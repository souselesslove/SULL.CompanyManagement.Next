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

namespace SULL.HospitalManagement.Services
{
    public partial class ItemHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemHistoryRepository _repository;//访问数据库
        private readonly IMessageProducer _messageProducer;
        private readonly IDashboardService _dashboardService;

        [ActivatorUtilitiesConstructor]
        public ItemHistoryService(
            IItemHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IMessageProducer messageProducer,
            IDashboardService dashboardService
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _messageProducer = messageProducer;
            _dashboardService = dashboardService;
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
                        // Item is bound to a Position(Slot) which has WarehouseId pointing to the Car(Position)
                        var slotPosition = db.Set<Position>().FirstOrDefault(x => x.ItemId == item.Id);
                        if (slotPosition != null && slotPosition.WarehouseId != null)
                        {
                            var carPosition = db.Set<Position>().FirstOrDefault(x => x.Id == slotPosition.WarehouseId);
                            if (carPosition != null) carName = carPosition.DisplayName;
                        }

                        string type = itemHistory.ChangeCounter > 0 ? "入库" : "出库";
                        string logMsg = $"[{deptName}]-[{carName}] 由 {operatorName} {type}{Math.Abs(itemHistory.ChangeCounter)}，当前库存{item.AvailableStock}";
                        _dashboardService.AddLog(item.OrganizationId.Value, type, logMsg);
                    }

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

        public WebResponseContent Statistics(PageDataOptions options)
        {
            // 1. 必填参数校验：OrganizationId 和 DepartmentId
            if (string.IsNullOrEmpty(options.Wheres))
            {
                 return new WebResponseContent().OK(null, new { rows = new List<object>(), total = 0 });
            }

            var wheres = JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
            long orgId = 0;
            long deptId = 0;

            if (wheres != null)
            {
                var orgParam = wheres.FirstOrDefault(x => x.Name == "OrganizationId");
                if (orgParam != null && long.TryParse(orgParam.Value?.ToString(), out long oid)) orgId = oid;

                var deptParam = wheres.FirstOrDefault(x => x.Name == "DepartmentId");
                if (deptParam != null && long.TryParse(deptParam.Value?.ToString(), out long did)) deptId = did;
            }

            if (orgId <= 0 || deptId <= 0)
            {
                 return new WebResponseContent().OK(null, new { rows = new List<object>(), total = 0 });
            }

            var db = _repository.DbContext;
            var query = from history in db.Set<ItemHistory>()
                        join item in db.Set<Item>() on history.ItemId equals item.Id
                        join detail in db.Set<ItemDetail>() on history.ItemDetailId equals detail.Id
                        where item.OrganizationId == orgId && item.DepartmentId == deptId
                        select new
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

            if (wheres != null)
            {
                foreach (var condition in wheres)
                {
                    if (string.IsNullOrEmpty(condition.Value)) continue;

                    if (condition.Name == "Name")
                    {
                        query = query.Where(x => x.ItemName.Contains(condition.Value));
                    }
                    else if (condition.Name == "CreationTime")
                    {
                        var times = condition.Value.Split(',');
                        if (times.Length >= 2 &&
                            DateTime.TryParse(times[0], out DateTime start) &&
                            DateTime.TryParse(times[1], out DateTime end))
                        {
                            query = query.Where(x => x.CreationTime >= start && x.CreationTime < end);
                        }
                    }
                }
            }

            var list = query.OrderByDescending(x => x.CreationTime).Take(50).ToList();

            return new WebResponseContent().OK(null, new
            {
                rows = list,
                total = list.Count
            });
        }

        public WebResponseContent SetAllStock(SaveModel saveModel)
        {
            if (!saveModel.MainData.TryGetValue("WarehouseId", out object warehouseIdObj) || warehouseIdObj == null)
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

            int successCount = 0;
            int failCount = 0;
            System.Collections.Generic.List<string> errorDetails = new System.Collections.Generic.List<string>();

            foreach (var itemId in itemIds)
            {
                if (itemId == null) continue;

                var item = db.Set<Item>().AsNoTracking().FirstOrDefault(x => x.Id == itemId);
                if (item == null) continue;

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
                            .Where(x => x.ItemId == itemId)
                            .OrderByDescending(x => x.CreationTime)
                            .FirstOrDefault();

                        if (detail == null)
                        {
                            // Create new ItemDetail if not found
                            var expireDate = DateTime.Now.AddMonths(6);
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
