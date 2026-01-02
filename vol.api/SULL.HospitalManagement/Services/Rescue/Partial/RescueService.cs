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
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public partial class RescueService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRescueRepository _repository;//访问数据库
        private readonly ICacheService _cache;
        [ActivatorUtilitiesConstructor]
        public RescueService(
            IRescueRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cache
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _cache = cache;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Rescue> GetPageData(PageDataOptions options)
        {
            // 检查缓存逻辑：当且仅当查询条件UserContextOrg/Dept + CartId时尝试走缓存
            // 此处简化判断：如果SearchParameters中只包含CartId（除了默认的权限过滤条件），则尝试读取缓存
            // 解析Wheres
            List<SearchParameters> searchParams = null;
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
                    if (_cache.Exists(key))
                    {
                         var cachedRescue = _cache.Get<Rescue>(key);
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
            }

            return base.GetPageData(options);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            // 强制设置当前用户的机构和科室
            saveModel.MainData["OrganizationId"] = UserContext.Current.OrganizationId;
            saveModel.MainData["DepartmentId"] = UserContext.Current.DepartmentId;

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

                // 缓存处理：写入新缓存，替换旧的
                string key = $"Rescue_Cart_{newEntity.CartId}";
                _cache.AddObject(key, newEntity);

                return new WebResponseContent(true);
            };

            return base.Add(saveModel);
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
            if (saveModel.MainData.TryGetValue("Status", out object statusObj) && statusObj != null)
            {
                int status = statusObj.ToString().GetInt();
                // 有效的状态只有0, 10, 20, 100
                if (status != 0 && status != 10 && status != 20 && status != 100)
                {
                    return new WebResponseContent().Error("非法操作");
                }

                if (saveModel.MainData.TryGetValue("Id", out object idObj))
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
                                    if (saveModel.MainData.TryGetValue(field, out object val))
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
                    _cache.AddObject(key, entity);
                }
                else
                {
                     // 其他状态更新（如修改基本信息），也同步更新缓存
                     _cache.AddObject(key, entity);
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
  }
}
