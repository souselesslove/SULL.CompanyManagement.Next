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
using Newtonsoft.Json;
using Yitter.IdGenerator;
using SULL.HospitalManagement.IServices;
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public partial class ItemHistoryService
    {
        private readonly IHospitalCacheService _hospitalCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemHistoryRepository _repository;
        private readonly IMessageProducer _messageProducer;
        private readonly IDashboardService _dashboardService;

        [ActivatorUtilitiesConstructor]
        public ItemHistoryService(
            IItemHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IMessageProducer messageProducer,
            IDashboardService dashboardService,
            IHospitalCacheService hospitalCache
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _messageProducer = messageProducer;
            _dashboardService = dashboardService;
            _hospitalCache = hospitalCache;
        }
        public override WebResponseContent Add(SaveModel saveModel)
        {
            long cartId = 0;
            if (saveModel.MainData.TryGetValue("CartId", out object? cartIdObj) && cartIdObj != null)
            {
                cartId = cartIdObj.ToString().GetLong();
                if (cartId <= 0)
                {
                    return new WebResponseContent().Error("请选择抢救车");
                }
            }
            
            AddOnExecuting = (itemHistory, context) =>
            {
                var db = _repository.DbContext;
                ItemDetail? itemDetail = null;

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
                    if (itemDetail != null)
                    {
                        if (item.LatestExpirationDate == null || itemDetail.ExpirationDate < item.LatestExpirationDate)
                        {
                            item.LatestExpirationDate = itemDetail.ExpirationDate;
                        }
                    }
                    
                    // 更新修改信息
                    item.LastModificationTime = DateTime.Now;
                    item.LastModifier = UserContext.Current.UserTrueName;
                    
                    db.Entry(item).State = EntityState.Modified;

                    // 3. 抢救中使用物品逻辑
                    if (itemHistory.ChangeCounter < 0 && cartId > 0)
                    {
                        // 优化：直接从缓存获取抢救状态
                        string key = $"Rescue_Cart_{cartId}";
                        var rescue = _hospitalCache.Get<Rescue>(key);
                        
                        if (rescue != null && rescue.Status == 0)
                        {
                            var rescueItem = db.Set<RescueItem>().FirstOrDefault(x => x.RescueId == rescue.Id && x.ItemId == item!.Id && x.ItemDetailId == itemDetail!.Id);
                            int qty = Math.Abs(itemHistory.ChangeCounter);
                            if (rescueItem != null)
                            {
                                rescueItem.Qty = (rescueItem.Qty ?? 0) + qty;
                                rescueItem.CreationTime = DateTime.Now;
                                rescueItem.Creator = UserContext.Current.UserTrueName;
                                rescueItem.CreatorId = UserContext.Current.UserId;
                                rescueItem.ItemDetailId = itemDetail!.Id;
                                rescueItem.ItemName = item!.Name;
                                db.Entry(rescueItem).State = EntityState.Modified;
                            }
                            else
                            {
                                rescueItem = new RescueItem
                                {
                                    Id = YitIdHelper.NextId(),
                                    RescueId = rescue.Id,
                                    ItemId = item!.Id,
                                    ItemDetailId = itemDetail!.Id,
                                    Qty = qty,
                                    CreationTime = DateTime.Now,
                                    Creator = UserContext.Current.UserTrueName,
                                    CreatorId = UserContext.Current.UserId,
                                    ItemName = item.Name
                                };
                                db.Set<RescueItem>().Add(rescueItem);
                                db.SaveChanges();
                            }
                        }
                    }
                    
                    // Log to Dashboard
                    if (item.OrganizationId > 0)
                    {
                        string deptName = "未知科室";
                        string carName = "未知位置";
                        string operatorName = UserContext.Current.UserTrueName;

                        if (item.DepartmentId > 0)
                        {
                            var dept = db.Set<OrganizationDepartment>().FirstOrDefault(x => x.Id == item.DepartmentId);
                            if (dept != null) deptName = dept.Name;
                        }

                        // Find Rescue Car info
                        var carNames = (from p in db.Set<Position>()
                                        join w in db.Set<Position>() on p.WarehouseId equals w.Id
                                        where p.ItemId == item.Id && p.Category == 110 && p.IsDeleted != true
                                        select w.DisplayName).Distinct().ToList();
                        string carInfo = carNames.Any() ? $"[{string.Join(",", carNames)}]" : "";
                        if (carNames.Any()) carName = carNames.First();

                        string type = itemHistory.ChangeCounter > 0 ? "入库" : "出库";
                        string logMsg = $"[{deptName}]-{carInfo} 由 {operatorName} {type}{Math.Abs(itemHistory.ChangeCounter)}，当前库存{item.AvailableStock}";
                        _dashboardService.AddLog(item.OrganizationId.Value, type, logMsg);
                    }

                    // 检查库存是否充足
                    if ((item.AvailableStock ?? 0) < item.Cardinality)
                    {
                        // 查找关联的抢救车信息（如果上面没查过的话，这里复用上面的结果）
                        var carNames = (from p in db.Set<Position>()
                                        join w in db.Set<Position>() on p.WarehouseId equals w.Id
                                        where p.ItemId == item.Id && p.Category == 110 && p.IsDeleted != true
                                        select w.DisplayName).Distinct().ToList();
                        string carInfo = carNames.Any() ? $"[{string.Join(",", carNames)}]" : "";

                        var todo = new SaveModel
                        {
                            MainData = new Dictionary<string, object>
                            {
                                { "Category", 20 }, // 库存预警Category=20
                                { "Description", $"{carInfo}物品[{item.Name}]规格[{item.Specification}]发生{(itemHistory.ChangeCounter > 0 ? "入库" : "出库")}{Math.Abs(itemHistory.ChangeCounter)}，当前库存[{item.AvailableStock}]，低于基数[{item.Cardinality}]，请及时补货" },
                                { "RelatedId", itemDetail!.Id },
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
            if (saveModel.MainData.TryGetValue("ItemDetailId", out object? itemDetailIdObj) && itemDetailIdObj != null)
            {
                string key = $"ItemHistory_Add_{itemDetailIdObj}";
                return KeyLock.Lock(key, () =>
                {
                    return base.Add(saveModel);
                });
            }
            return base.Add(saveModel);
        }

        public WebResponseContent Statistics(PageDataOptions options)
        {
            // 1. 必填参数校验：OrganizationId 和 DepartmentId
            long orgId = UserContext.Current.OrganizationId;
            long deptId = UserContext.Current.DepartmentId;

            if (orgId <= 0 || deptId <= 0)
            {
                return new WebResponseContent().Error("非法操作");
            }

            // 2. 解析查询条件并应用到 ItemHistory 基础过滤
            ValidatePageOptions(options, out IQueryable<ItemHistory> historyQuery);

            // 3. 安全校验：确保查询条件中指定的机构和部门与当前用户一致
            if (options.Filter != null && (options.Filter.Any(x => x.Name == "OrganizationId" && x.Value != orgId.ToString()) ||
                                         options.Filter.Any(x => x.Name == "DepartmentId" && x.Value != deptId.ToString())))
            {
                return new WebResponseContent().Error("非法操作");
            }

            var db = _repository.DbContext;
            // 4. 组装连接查询逻辑
            var query = from history in historyQuery
                        join item in db.Set<Item>() on history.ItemId equals item.Id
                        join detail in db.Set<ItemDetail>() on history.ItemDetailId equals detail.Id
                        where item.OrganizationId == orgId && item.DepartmentId == deptId
                        select new ItemHistoryStatistics
                        {
                            ItemId = item.Id,
                            ItemName = item.Name,
                            Specification = item.Specification,
                            RegisterCode = detail.RegisterCode,
                            ExpirationDate = detail.ExpirationDate,
                            ChangeCounter = history.ChangeCounter,
                            Stock = history.Stock,
                            CreationTime = history.CreationTime
                        };

            // 5. 应用未被 historyQuery 处理的其他条件（主要是针对连接表的条件，如 Name 匹配 ItemName）
            if (options.Filter != null && options.Filter.Count > 0)
            {
                var resultProperties = typeof(ItemHistoryStatistics).GetProperties();
                var itemHistoryProperties = typeof(ItemHistory).GetProperties();
                foreach (var x in options.Filter)
                {
                    if (string.IsNullOrEmpty(x.Value)) continue;

                    // 映射特殊字段：前端传 Name -> 匹配结果集的 ItemName
                    string targetName = x.Name;
                    if (targetName == "Name") targetName = "ItemName";

                    // 检查该属性是否在结果集中且未在 ItemHistory 级别处理过
                    if (resultProperties.Any(p => p.Name == targetName) && 
                        !itemHistoryProperties.Any(p => p.Name == targetName))
                    {
                        var expressionType = x.DisplayType.GetLinqCondition();
                        query = query.Where(targetName.CreateExpression<ItemHistoryStatistics>(x.Value, expressionType));
                    }
                }
            }

            // 获取总数
            int total = query.Count();

            // 分页并返回
            var list = query.OrderByDescending(x => x.CreationTime)
                            .Skip((options.Page - 1) * options.Rows)
                            .Take(options.Rows)
                            .ToList();

            return new WebResponseContent().OK(null, new
            {
                rows = list,
                total = total
            });
        }

        /// <summary>
        /// 统计查询结果模型
        /// </summary>
        public class ItemHistoryStatistics
        {
            public long ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public string Specification { get; set; } = string.Empty;
            public string RegisterCode { get; set; } = string.Empty;
            public DateTime? ExpirationDate { get; set; }
            public int ChangeCounter { get; set; }
            public int Stock { get; set; }
            public DateTime? CreationTime { get; set; }
        }

        public WebResponseContent SetAllStock(SaveModel saveModel)
        {
            if (!saveModel.MainData.TryGetValue("WarehouseId", out object? warehouseIdObj) || warehouseIdObj == null)
            {
                 return new WebResponseContent().Error("无效的参数: WarehouseId");
            }
            long warehouseId = warehouseIdObj.ToString().GetLong();
            
            if (warehouseId <= 0) return new WebResponseContent().Error("无效的仓库编号");

            var db = _repository.DbContext;
            // 1. 查询当前仓库下所有位置关联的物品ID
            var itemIds = db.Set<Position>()
                .Where(x => x.WarehouseId == warehouseId && x.ItemId != null)
                .Select(x => x.ItemId)
                .Distinct()
                .ToList();

            if (itemIds.Count == 0)
            {
                return new WebResponseContent().Error("当前仓库未配置任何物品，无需补货");
            }

            var items = db.Set<Item>().AsNoTracking().Where(x => itemIds.Contains(x.Id)).ToList();
            int successCount = 0;
            int failCount = 0;
            System.Collections.Generic.List<string> errorDetails = new System.Collections.Generic.List<string>();

            foreach (var item in items)
            {
                int available = item.AvailableStock ?? 0;
                int cardinality = item.Cardinality;

                if (available < cardinality)
                {
                    int need = cardinality - available;
                    if (need > 0)
                    {
                        // 寻找最新且有效的ItemDetail
                        var detail = db.Set<ItemDetail>()
                            .AsNoTracking()
                            .Where(x => x.ItemId == item.Id)
                            .OrderByDescending(x => x.CreationTime)
                            .FirstOrDefault();

                        if (detail == null)
                        {
                            // 物品过期时间设置：设备按3年，药品耗材等其他按6个月
                            int addMonths = (item.Category == 30) ? 36 : 6;
                            var expireDate = DateTime.Now.AddMonths(addMonths);
                            
                            detail = new ItemDetail
                            {
                                Id = YitIdHelper.NextId(),
                                ItemId = item.Id,
                                RegisterCode = expireDate.ToString("yyyyMMdd"),
                                ExpirationDate = expireDate,
                                Stock = 0,
                                CreationTime = DateTime.Now,
                                Creator = UserContext.Current.UserTrueName,
                                CreatorId = UserContext.Current.UserId,
                                OrderBy = 0,
                                IsDeleted = false,
                                Status = 1
                            };
                            // Also set Creator/CreateTime if BaseEntity fields are different, typically SetCreateDefaultVal() does this but manual here is fine
                            detail.SetCreateDefaultVal(); 
                            
                            db.Set<ItemDetail>().Add(detail);
                            db.SaveChanges();
                            db.Entry(detail).State = EntityState.Detached;
                        }

                        if (detail != null)
                        {
                            // 构造SaveModel
                            var saveModelI = new SaveModel
                            {
                                MainData = new Dictionary<string, object>
                                {
                                    { "ItemId", item.Id },
                                    { "ItemDetailId", detail.Id },
                                    { "ChangeCounter", need },
                                    { "ChangeType", "一键补货" },
                                    { "Remark", $"一键补库自动生成，补货数量:{need}" }
                                }
                            };

                            // 调用Add增加库存
                            var result = this.Add(saveModelI);
                            if (result.Status)
                            {
                                successCount++;
                            }
                            else
                            {
                                failCount++;
                                errorDetails.Add($"物品[{item.Name}]补货失败: {result.Message}");
                            }
                        }
                    }
                }
            }

            string msg = $"一键补库完成。成功: {successCount}, 失败: {failCount}。";
            if (failCount > 0)
            {
                msg += " 详情: " + string.Join("; ", errorDetails.Take(5)) + (errorDetails.Count > 5 ? "..." : "");
                return new WebResponseContent().Error(msg); 
            }
            return new WebResponseContent().OK(msg);
        }
}
}
