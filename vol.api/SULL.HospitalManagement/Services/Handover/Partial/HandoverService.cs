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

namespace SULL.HospitalManagement.Services
{

    public partial class HandoverService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHandoverRepository _repository; //访问数据库
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public HandoverService(
            IHandoverRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IMessageProducer messageProducer)
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _messageProducer = messageProducer;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
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
                              && p.ItemId != null
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
                        }).ToList();

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
            if (saveModel.MainData.TryGetValue("AuditStatus", out object status) && status != null)
            {
                string statusStr = status.ToString();
                if (statusStr == "10" || statusStr == "20")
                {
                    if (saveModel.MainData.TryGetValue("Id", out object id) && id != null &&
                        long.TryParse(id.ToString(), out long handoverId))
                    {
                        var entity = _repository.FindFirst(x => x.Id == handoverId);
                        if (entity == null)
                        {
                            return new WebResponseContent().Error("数据不存在");
                        }

                        if (statusStr == "10")
                        {
                            if (entity.AuditStatus != 1)
                            {
                                return new WebResponseContent().Error("非法操作");
                            }

                            saveModel.MainData["operate"] = "complete";
                            saveModel.MainData["Kind"] = 1;
                            _messageProducer.PublishAsync(saveModel, Define.ExchangeName, Define.TodoQueue).GetAwaiter()
                                .GetResult();
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
    }
}
