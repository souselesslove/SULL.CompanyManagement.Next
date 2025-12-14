/*
 *所有关于ItemHistory类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ItemHistoryService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.RabbitMQ;

namespace SULL.HospitalManagement.Services
{
    public partial class ItemHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemHistoryRepository _repository;//访问数据库
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public ItemHistoryService(
            IItemHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IMessageProducer messageProducer
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _messageProducer = messageProducer;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }
        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuting = (itemHistory, context) =>
            {
                var db = _repository.DbContext;
                ItemDetail itemDetail = null;

                // 1. 处理ItemDetail
                if (itemHistory.ItemDetailId > 0)
                {
                    itemDetail = db.Set<ItemDetail>().FirstOrDefault(x => x.Id == itemHistory.ItemDetailId);
                    if (itemDetail != null)
                    {
                        itemDetail.Stock = (itemDetail.Stock ?? 0) + itemHistory.ChangeCounter;
                        itemDetail.LatestChangeCounter = itemHistory.ChangeCounter;
                        itemDetail.LastModificationTime = DateTime.Now;
                        itemDetail.LastModifierId = UserContext.Current.UserId;
                        itemDetail.LastModifier = UserContext.Current.UserTrueName;
                        itemDetail.LatestHistoryId = itemHistory.Id;
                        db.Entry(itemDetail).State = EntityState.Modified;
                    }
                }
                
                if (itemDetail == null) 
                {
                     return new WebResponseContent().Error("无效的物品规格");
                }

                // 更新ItemHistory的Stock快照
                itemHistory.Stock = itemDetail.Stock ?? 0;

                // 2. 更新Item总信息
                var item = db.Set<Item>().FirstOrDefault(x => x.Id == itemHistory.ItemId);
                if (item != null)
                {
                    item.AvailableStock = (item.AvailableStock ?? 0) + itemHistory.ChangeCounter;
                    
                    // 更新修改信息
                    item.LastModificationTime = DateTime.Now;
                    item.LastModifier = UserContext.Current.UserTrueName;
                    
                    db.Entry(item).State = EntityState.Modified;

                    // 检查库存是否充足
                    if ((item.AvailableStock ?? 0) < item.Cardinality)
                    {
                        var todo = new SaveModel
                        {
                            MainData = new Dictionary<string, object>
                            {
                                { "Category", 20 }, // 库存预警Category=20
                                { "Description", $"物品[{item.Name}]规格[{item.Specification}]发生{(itemHistory.ChangeCounter > 0 ? "入库" : "出库")}{Math.Abs(itemHistory.ChangeCounter)}，当前库存[{item.AvailableStock}]，低于基数[{item.Cardinality}]，请及时补货" },
                                { "RelatedId", itemDetail.Id },
                            }
                        };
                        _messageProducer.PublishAsync(todo, Define.ExchangeName, Define.TodoQueue);
                    }
                }
                else
                {
                    return new WebResponseContent().Error("物品(Item)不存在");
                }

                return new WebResponseContent(true);
            };
            // 使用细粒度锁防止并发库存更新不一致
            if (saveModel.MainData.TryGetValue("ItemDetailId", out object itemDetailIdObj) && itemDetailIdObj != null)
            {
                string key = $"ItemHistory_Add_{itemDetailIdObj}";
                return KeyLock.Lock(key, () =>
                {
                    return base.Add(saveModel);
                });
            }
            return base.Add(saveModel);
        }
}
}
