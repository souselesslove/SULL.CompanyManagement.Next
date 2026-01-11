/*
 *所有关于Rescue类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*RescueService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
*/
using VOL.Core.BaseProvider;
using VOL.Core.Extensions.AutofacManager;
using VOL.Entity.DomainModels;
using System.Linq;
using VOL.Core.Utilities;
using System.Linq.Expressions;
using VOL.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using SULL.HospitalManagement.IRepositories;

using VOL.Core.ManageUser;
using Yitter.IdGenerator;
using SULL.HospitalManagement.IServices;
using VOL.Core.CacheManager;
using VOL.Core.RabbitMQ;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SULL.HospitalManagement.IServices.Dtos;

namespace SULL.HospitalManagement.Services
{
    public partial class RescueService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRescueRepository _repository;//访问数据库
        private readonly IHospitalCacheService _hospitalCache;
        private readonly IMessageProducer _messageProducer;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IOrganizationUserRepository _organizationUserRepository;
        [ActivatorUtilitiesConstructor]
        public RescueService(
            IRescueRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHospitalCacheService hospitalCache,
            IMessageProducer messageProducer,
            IUserAccountRepository userAccountRepository,
            IOrganizationUserRepository organizationUserRepository
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _hospitalCache = hospitalCache;
            _messageProducer = messageProducer;
            _userAccountRepository = userAccountRepository;
            _organizationUserRepository = organizationUserRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Rescue> GetPageData(PageDataOptions options)
        {
            // 检查缓存逻辑：当且仅当查询条件UserContextOrg/Dept + CartId时尝试走缓存
            // 此处简化判断：如果SearchParameters中只包含CartId（除了默认的权限过滤条件），则尝试读取缓存
            // 解析Wheres
            List<SearchParameters>? searchParams = null;
            if (!string.IsNullOrEmpty(options.Wheres))
            {
                try
                {
                    searchParams = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                }
                catch { }
            }

            if (options.Rows == 1 && searchParams != null && searchParams.Count == 1 && searchParams[0].Name == "CartId")
            {
                string cartId = searchParams[0].Value;
                if (!string.IsNullOrEmpty(cartId))
                {
                    string key = $"Rescue_Cart_{cartId}";
                    var cachedRescue = _hospitalCache.Get<Rescue>(key);
                    if (cachedRescue != null)
                    {
                        // 构造PageGridData返回
                        return new PageGridData<Rescue>
                        {
                            rows = new List<Rescue> { cachedRescue },
                            total = 1
                        };
                    }
                }
            }

            return base.GetPageData(options);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            // 强制设置当前用户的机构和科室
            saveModel.MainData["OrganizationId"] = UserContext.Current.OrganizationId;
            saveModel.MainData["DepartmentId"] = UserContext.Current.DepartmentId;

            long cartId = saveModel.MainData.GetValueOrDefault("CartId").GetLong();
            if (cartId <= 0)
            {
                return new WebResponseContent().Error("抢救车编号不能为空");
            }

            string lockKey = $"Rescue_Add_{cartId}";
            WebResponseContent? result = null;

            AddOnExecuted = (newEntity, list) =>
            {
                var db = _repository.DbContext;
                db.Set<WarningLogs>().Add(new WarningLogs
                {
                    Id = YitIdHelper.NextId(),
                    Category = 110,
                    Description = $"[{newEntity.CartName}]正在抢救病人[{newEntity.Name}]",
                    OrganizationId = newEntity.OrganizationId,
                    DepartmentId = newEntity.DepartmentId,
                    RelatedId = newEntity.Id,
                    CreationTime = DateTime.Now,
                    IsFinished = false
                });
                db.SaveChanges();

                // 发送通知
                NotifyDepartmentManagersAsync(newEntity.OrganizationId ?? 0, newEntity.DepartmentId, "抢救开始通知", $"[{newEntity.CartName}]正在抢救病人[{newEntity.Name}]").GetAwaiter().GetResult();

                // 缓存处理：写入新缓存，替换旧的
                string key = $"Rescue_Cart_{newEntity.CartId}";
                _hospitalCache.Set(key, newEntity);

                return new WebResponseContent(true);
            };

            KeyLock.Lock(lockKey, () =>
            {
                // 检测是否存在正在抢救中的记录 (Status != 10 && Status != 20 && Status != 100)
                // 简化判断：Status == 0 为正在抢救
                bool exists = _repository.Exists(x => x.CartId == cartId && (x.Status == 0 || x.Status == null) && x.IsDeleted != true);
                if (exists)
                {
                    result = new WebResponseContent().Error("该抢救车已有正在进行中的抢救任务，请先结束当前抢救。");
                    return;
                }

                result = base.Add(saveModel);
            });

            return result ?? new WebResponseContent().Error("操作失败");
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            // 确保更新时不修改 OrganizationId and DepartmentId
            if (saveModel.MainData.ContainsKey("OrganizationId"))
            {
                saveModel.MainData.Remove("OrganizationId");
            }
            if (saveModel.MainData.ContainsKey("DepartmentId"))
            {
                saveModel.MainData.Remove("DepartmentId");
            }
            // 新增逻辑：Status=10 or 20 时，仅更新Status和自动计算的Duration
            if (saveModel.MainData.TryGetValue("Status", out object? statusObj) && statusObj != null)
            {
                int status = statusObj.ToString().GetInt();
                // 有效的状态只有0, 10, 20, 100
                if (status != 0 && status != 10 && status != 20 && status != 100)
                {
                    return new WebResponseContent().Error("非法操作");
                }

                if (saveModel.MainData.TryGetValue("Id", out object? idObj))
                {
                    var id = idObj.ToString().GetLong();
                    var record = _repository.FindFirst(x => x.Id == id);
                    if (record != null)
                    {
                        if (status < (record.Status ?? 0))
                        {
                            return new WebResponseContent().Error("非法操作");
                        }
                        
                        // 状态为10或20时的特殊逻辑
                        if (status == 10 || status == 20)
                        {
                            if (record.StartTime != null)
                            {
                                var duration = (int)(DateTime.Now - record.StartTime.Value).TotalMinutes;
                                
                                // 保存关键业务字段，防止在状态变更时被 Clear 掉
                                string[] fieldsToKeep = { "Changes", "Check", "Diagnosis", "Process", "Effect", "Remark" };
                                var keepData = new Dictionary<string, object>();
                                foreach (var field in fieldsToKeep)
                                {
                                    if (saveModel.MainData.TryGetValue(field, out object? val))
                                    {
                                        keepData[field] = val;
                                    }
                                }

                                // 清除其他字段，仅保留Id, Status, Duration, Name, CartId, CartName 等核心控制字段
                                saveModel.MainData.Clear();
                                saveModel.MainData["Id"] = id;
                                saveModel.MainData["Status"] = status;
                                saveModel.MainData["Duration"] = duration;
                                saveModel.MainData["Name"] = record.Name; // 必填校验需要
                                saveModel.MainData["CartId"] = record.CartId;
                                saveModel.MainData["CartName"] = record.CartName;
                                saveModel.MainData["EndTime"] = DateTime.Now;

                                // 将保留的业务数据重新放回
                                foreach (var item in keepData)
                                {
                                    saveModel.MainData[item.Key] = item.Value;
                                }
                            }
                        }
                    }
                }
            }

            UpdateOnExecuted = (entity, obj, x, x1) =>
            {
                string key = $"Rescue_Cart_{entity.CartId}";

                if (entity.Status == 10 || entity.Status == 20 || entity.Status == 100)
                {
                    var db = _repository.DbContext;
                    var logs = db.Set<WarningLogs>().Where(x => x.Category == 110 && x.RelatedId == entity.Id && x.IsFinished == false).ToList();
                    foreach (var log in logs)
                    {
                        log.IsFinished = true;
                        // log.ModificationTime = DateTime.Now; 
                    }
                    if (logs.Any())
                    {
                         db.SaveChanges();
                    }

                    // 缓存处理：始终保留/更新缓存
                    _hospitalCache.Set(key, entity);

                    if (entity.Status == 10 || entity.Status == 20 || entity.Status == 100)
                    {
                        // 结束抢救时也记录一个日志
                        db.Set<WarningLogs>().Add(new WarningLogs
                        {
                            Id = YitIdHelper.NextId(),
                            Category = 110,
                            Description = $"[{entity.CartName}]抢救病人[{entity.Name}]已结束",
                            OrganizationId = entity.OrganizationId,
                            DepartmentId = entity.DepartmentId,
                            RelatedId = entity.Id,
                            CreationTime = DateTime.Now,
                            IsFinished = true
                        });
                        db.SaveChanges();

                        // 发送通知
                        NotifyDepartmentManagersAsync(entity.OrganizationId ?? 0, entity.DepartmentId, "抢救结束通知", $"[{entity.CartName}]抢救病人[{entity.Name}]已结束").GetAwaiter().GetResult();
                    }
                }
                else
                {
                     // 其他状态更新（如修改基本信息），也同步更新缓存
                     _hospitalCache.Set(key, entity);
                }
                return new WebResponseContent(true);
            };

            return base.Update(saveModel);
        }

        public WebResponseContent Statistics(PageDataOptions options)
        {
            // 默认参数校验，期望options.Wheres里有CartId
            long cartId = 0;
            if (!string.IsNullOrEmpty(options.Wheres))
            {
                var wheres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                if (wheres != null)
                {
                    var cartParam = wheres.FirstOrDefault(x => x.Name == "CartId");
                    if (cartParam != null && long.TryParse(cartParam.Value, out long cid))
                    {
                        cartId = cid;
                    }
                }
            }

            if (cartId <= 0)
            {
                 return new WebResponseContent().OK(null, new { monthCount = 0, lastTime = (DateTime?)null });
            }

            var db = _repository.DbContext;
            // 统计本月的抢救次数
            var now = DateTime.Now;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var query = db.Set<Rescue>().Where(x => x.CartId == cartId 
                && x.CreationTime >= monthStart 
                && x.IsDeleted != true
                && x.OrganizationId == UserContext.Current.OrganizationId
                && x.DepartmentId == UserContext.Current.DepartmentId);
            
            int count = query.Count();
            
            // 最近一次抢救时间
            var lastRescue = db.Set<Rescue>()
                .Where(x => x.CartId == cartId 
                    && x.IsDeleted != true
                    && x.OrganizationId == UserContext.Current.OrganizationId
                    && x.DepartmentId == UserContext.Current.DepartmentId)
                .OrderByDescending(x => x.CreationTime)
                .Select(x => x.CreationTime)
                .FirstOrDefault();

            // 如果没有记录，lastRescue会是默认值MinValue，需要处理
            DateTime? lastTime = null;
            if (lastRescue != DateTime.MinValue)
            {
                lastTime = lastRescue;
            }

            return new WebResponseContent().OK(null, new { monthCount = count, lastTime = lastTime });
        }

        private async Task NotifyDepartmentManagersAsync(long orgId, long deptId, string title, string content)
        {
            if (orgId <= 0 || deptId <= 0) return;

            // 1. 查找本科室 Level >= 30 的用户
            var managerUserIds = await _organizationUserRepository.DbContext.Set<OrganizationUser>()
                .Where(x => x.OrganizationId == orgId && x.DepartmentId == deptId && x.Level >= 30 && x.IsDeleted != true)
                .Select(x => x.UserId)
                .ToListAsync();

            if (!managerUserIds.Any()) return;

            var clientIds = new List<string>();
            foreach (var userId in managerUserIds)
            {
                if (userId == null || userId <= 0) continue;

                // 2. 尝试从缓存获取账号
                var cachedAccount = _hospitalCache.GetUserAccount<UserAccount>(userId.Value, 10);
                string? userAccount = cachedAccount?.Account;

                if (string.IsNullOrEmpty(userAccount))
                {
                    // 3. 查库
                    var account = await _userAccountRepository.DbContext.Set<UserAccount>()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted != true && x.Category == 10);
                    if (account != null)
                    {
                        userAccount = account.Account;
                        _hospitalCache.SetUserAccount(userId.Value, 10, account);
                    }
                }

                if (!string.IsNullOrEmpty(userAccount))
                {
                    clientIds.Add(userAccount);
                }
            }

            if (clientIds.Any())
            {
                // 4. 发送通知
                var msg = new NotificationMessage
                {
                    ClientIds = clientIds,
                    Title = title,
                    Content = content
                };
                await _messageProducer.PublishAsync(msg, Define.ExchangeName, Define.NotificationQueue);
            }
        }
    }
}
