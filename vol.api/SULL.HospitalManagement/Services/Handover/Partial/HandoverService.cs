/*
 *所有关于Handover类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*HandoverService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using SULL.HospitalManagement.IServices;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;
using Yitter.IdGenerator;
using VOL.Core.CacheManager;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using SULL.HospitalManagement.IServices.Dtos;

namespace SULL.HospitalManagement.Services
{

    public partial class HandoverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHandoverRepository _repository; //访问数据库
        private readonly IMessageProducer _messageProducer;
        private readonly IDashboardService _dashboardService;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IHospitalCacheService _hospitalCache;

        [ActivatorUtilitiesConstructor]
        public HandoverService(
            IHandoverRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, 
            IMessageProducer messageProducer,
            IDashboardService dashboardService,
            IUserAccountRepository userAccountRepository,
            IHospitalCacheService hospitalCache)
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _messageProducer = messageProducer;
            _repository = dbRepository;
            _dashboardService = dashboardService;
            _userAccountRepository = userAccountRepository;
            _hospitalCache = hospitalCache;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            var startTime = saveModel.MainData["ShiftTime"].GetDateTime();
            saveModel.MainData.Add("AuditStatus", 0);
            base.AddOnExecuting = (obj, o) =>
            {
                var exist = _repository.FindFirst(x => x.CreatorId == UserContext.Current.UserId
                                                       && x.Category == obj.Category
                                                       && (string.IsNullOrWhiteSpace(obj.ShiftName) ||
                                                           x.ShiftName == obj.ShiftName)
                                                       && x.RelatedId == obj.RelatedId
                                                       && x.IsDeleted != true
                                                       && (startTime == null || (x.ShiftTime >= startTime.Value.Date
                                                           && x.ShiftTime < startTime.Value.Date.AddDays(1))));
                return exist == null ? new WebResponseContent(true) : new WebResponseContent().Error("重复交接班");
            };

            base.AddOnExecuted = (handover, response) =>
            {
                // 只针对抢救车交接班处理
                if (handover.Category == 10)
                {
                    // 1. 获取该抢救车（Position）绑定的所有物品
                    // 抢救车编号通过 RelatedId (对应Position的warehouseId) 获取
                    // 查询条件: OrganizationId, DepartmentId, WarehouseId = handover.RelatedId, Category = 110 (具体绑定的物品位置)

                    var db = _repository.DbContext;
                    var relatedId = handover.RelatedId; // 抢救车ID

                    // 使用 Join 联表查询 Position -> ItemDetail -> Item
                    //以此一次性获取所需的所有信息
                    var query = from p in db.Set<Position>()
                        join d in db.Set<ItemDetail>().Where(x => x.IsDeleted != true) on p.ItemId equals d.ItemId into
                            details
                        from d in details.DefaultIfEmpty()
                        where p.OrganizationId == handover.OrganizationId
                              && p.DepartmentId == handover.DepartmentId
                              && p.WarehouseId == relatedId
                              && p.Category == 110 // 绑定物品的层级/位置
                              && p.IsDeleted != true
                              && p.ItemId > 0
                        select new
                        {
                            ItemId = p.ItemId ?? 0,
                            ItemDetailId = d != null ? d.Id : 0,
                            ItemName = p.ItemName,
                            ItemCounter = d != null ? d.Stock : 0,
                            ExpirationDate = d != null ? (DateTime?)d.ExpirationDate : null
                        };

                    var resultList = query.ToList();

                    // 批量写入 HandoverItemCheck
                    if (resultList.Count > 0)
                    {
                        var detailsToAdd = resultList.Select(x => new HandoverItemCheck
                        {
                            Id = YitIdHelper.NextId(),
                            OrganizationId = handover.OrganizationId,
                            DepartmentId = handover.DepartmentId,
                            HandoverId = handover.Id,
                            ItemId = x.ItemId,
                            ItemDetailId = x.ItemDetailId,
                            ItemName = x.ItemName,
                            ItemCounter = x.ItemCounter ?? 0,
                            ItemRealCounter = x.ItemCounter ?? 0, // 默认为系统数量
                            ExpirationDate = x.ExpirationDate,
                            IsDeleted = false
                        }).Where(x => x.ItemId > 0).ToList();

                        db.Set<HandoverItemCheck>().AddRange(detailsToAdd);
                        db.SaveChanges();
                    }
                }

                return new WebResponseContent(true);
            };

            // 使用细粒度锁防止重复提交
            return KeyLock.Lock($"Handover_Add_{UserContext.Current.UserId}_{saveModel.MainData["RelatedId"]}",
                () => { return base.Add(saveModel); });
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            // TODO 可能存在数据不一致问题
            if (saveModel.MainData.TryGetValue("AuditStatus", out object? status) && status != null)
            {
                string? statusStr = status.ToString();
                if (statusStr == "1" || statusStr == "10" || statusStr == "20")
                {
                    if (saveModel.MainData.TryGetValue("Id", out object? id) && id != null &&
                        long.TryParse(id.ToString(), out long handoverId))
                    {
                        var entity = _repository.FindFirst(x => x.Id == handoverId);
                        if (entity == null)
                        {
                            return new WebResponseContent().Error("数据不存在");
                        }

                        if (statusStr == "1")
                        {
                            if (entity.AuditStatus != 0)
                            {
                                return new WebResponseContent().Error("非法操作");
                            }
                            // 向接班人推送通知
                            if (entity.ReceiverId > 0)
                            {
                                NotifyReceiverAsync(entity).GetAwaiter().GetResult();
                            }
                        }
                        else if (statusStr == "10")
                        {
                            if (entity.AuditStatus != 1)
                            {
                                return new WebResponseContent().Error("非法操作");
                            }

                            saveModel.MainData["operate"] = "complete";
                            saveModel.MainData["Kind"] = 1;
                            _messageProducer.PublishAsync(saveModel, Define.ExchangeName, Define.TodoQueue).GetAwaiter()
                                .GetResult();

                            // Log to Dashboard
                            if (entity.CreationTime != null)
                            {
                                var endTime = DateTime.Now;
                                var duration = (int)(endTime - entity.CreationTime.Value).TotalSeconds;
                                var durationStat = new DurationStatistics
                                {
                                    Id = YitIdHelper.NextId(),
                                    OrganizationId = entity.OrganizationId,
                                    DepartmentId = entity.DepartmentId,
                                    Category = 10,
                                    RelatedId = handoverId,
                                    CreatorId = UserContext.Current.UserId,
                                    Creator = UserContext.Current.UserTrueName,
                                    CreationTime = endTime,
                                    StartTime = entity.CreationTime,
                                    EndTime = endTime,
                                    Duration = duration,
                                    Remark = ""
                                };
                                _repository.DbContext.Set<DurationStatistics>().Add(durationStat);
                                _repository.DbContext.SaveChanges();
                            }
                            
                            // Log to Dashboard
                            if (entity.OrganizationId > 0)
                            {
                                var db = _repository.DbContext;
                                string deptName = "未知科室";
                                string carName = "未知位置";
                                string operatorName = UserContext.Current.UserTrueName;

                                if (entity.DepartmentId > 0)
                                {
                                    var dept = db.Set<OrganizationDepartment>().FirstOrDefault(x => x.Id == entity.DepartmentId);
                                    if (dept != null) deptName = dept.Name;
                                }

                                if (entity.RelatedId > 0)
                                {
                                    var car = db.Set<Position>().FirstOrDefault(x => x.Id == entity.RelatedId);
                                    if (car != null) carName = car.DisplayName;
                                }

                                string logMsg = $"[{deptName}]-[{carName}] 由 {operatorName} 完成交接班";
                                _dashboardService.AddLog(entity.OrganizationId.Value, "交接班", logMsg);

                                // 向交接班创建人推送通知
                                if (entity.CreatorId > 0 && entity.CreatorId != UserContext.Current.UserId)
                                {
                                    string notifyTitle = "交接班完成通知";
                                    string notifyContent = $"您提交的[{carName}]交接班已由 {operatorName} 确认完成。";
                                    var notifyPayload = new { Id = entity.Id, Category = entity.Category, RelatedId = entity.RelatedId };
                                    NotifyUserAsync(entity.CreatorId.Value, notifyTitle, notifyContent, notifyPayload).GetAwaiter().GetResult();
                                }
                            }
                        }
                        else if (statusStr == "20")
                        {
                            if (entity.AuditStatus != 10)
                            {
                                return new WebResponseContent().Error("非法操作");
                            }

                            saveModel.MainData["Kind"] = 1;
                            saveModel.MainData["AuditUserId"] = UserContext.Current.UserId;
                            saveModel.MainData["AuditUser"] = UserContext.Current.UserTrueName;
                            saveModel.MainData["AuditTime"] = DateTime.Now;
                        }
                    }
                }
            }
            return base.Update(saveModel);
        }
        public WebResponseContent GetMissingShifts(IServices.Handover.Dtos.MissingShiftInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.ShiftNames) || input.RelatedId <= 0)
            {
                return new WebResponseContent(false);
            }

            var targetDate = input.ShiftTime.Date;
            var nextDate = targetDate.AddDays(1);

            // 查询当天已存在的班次
            // RelatedId=抢救车编号
            var existingShifts = _repository.Find(x => x.RelatedId == input.RelatedId
                                                       && x.ShiftTime >= targetDate
                                                       && x.ShiftTime < nextDate)
                .Select(x => x.ShiftName)
                .ToList();

            var inputShiftList = input.ShiftNames.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            // 找出不存在的班次
            var missingShifts = inputShiftList.Where(s => !existingShifts.Contains(s)).ToList();

            if (missingShifts.Any())
            {
                var db = _repository.DbContext;
                var position = db.Set<Position>().FirstOrDefault(x => x.Id == input.RelatedId);
                if (position != null)
                {
                    foreach (var shiftName in missingShifts)
                    {
                        var desc = $"[{targetDate:yyyy-MM-dd}][{shiftName}]交接班缺失";
                        db.Set<WarningLogs>().Add(new WarningLogs
                            {
                                Id = YitIdHelper.NextId(),
                                Category = 10,
                                Description = desc,
                                OrganizationId = position.OrganizationId,
                                DepartmentId = position.DepartmentId,
                                RelatedId = input.RelatedId,
                                CreationTime = DateTime.Now,
                                Remark = "",
                                IsFinished = false
                            });
                    }
                    db.SaveChanges();
                }
            }

            return new WebResponseContent(true);
        }

        private async Task NotifyReceiverAsync(Handover handover)
        {
            if (handover.ReceiverId == null || handover.ReceiverId <= 0) return;

            string carName = "";
            var position = await _repository.DbContext.Set<Position>().FirstOrDefaultAsync(x => x.Id == handover.RelatedId);
            if (position != null) carName = $"[{position.DisplayName}]";

            string title = "交接班确认提醒";
            string content = $"{handover.Creator} 已向您提交了{carName}交接班，请进入系统确认。";
            var payload = new { Id = handover.Id, Category = handover.Category, RelatedId = handover.RelatedId };

            await NotifyUserAsync(handover.ReceiverId.Value, title, content, payload);
        }

        private async Task NotifyUserAsync(int userId, string title, string content, object? payload = null)
        {
            if (userId <= 0) return;

            // 1. 尝试从缓存获取
            var cachedAccount = _hospitalCache.GetUserAccount<UserAccount>(userId, 10);
            string? userAccount = cachedAccount?.Account;

            if (string.IsNullOrEmpty(userAccount))
            {
                // 2. 查库
                var account = await _userAccountRepository.DbContext.Set<UserAccount>()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted != true && x.Category == 10);
                if (account != null)
                {
                    userAccount = account.Account;
                    _hospitalCache.SetUserAccount(userId, 10, account);
                }
            }

            if (!string.IsNullOrEmpty(userAccount))
            {
                // 3. 发布到消息队列异步处理 (Phase 3)
                var msg = new NotificationMessage
                {
                    ClientIds = new List<string> { userAccount },
                    Title = title,
                    Content = content,
                    Payload = payload ?? new object()
                };
                await _messageProducer.PublishAsync(msg, Define.ExchangeName, Define.NotificationQueue);
            }
        }
    }
}
