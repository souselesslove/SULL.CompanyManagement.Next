/*
 *所有关于Item类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ItemService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using SULL.HospitalManagement.IRepositories;
using Yitter.IdGenerator;
using SULL.HospitalManagement.IServices;
using VOL.Core.CacheManager;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;
using SULL.HospitalManagement.IServices.Dtos;


namespace SULL.HospitalManagement.Services
{
    public partial class ItemService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemRepository _repository; //访问数据库
        private readonly IHospitalCacheService _hospitalCache;
        private readonly IMessageProducer _messageProducer;
        private readonly IPositionService _positionService;

        [ActivatorUtilitiesConstructor]
        public ItemService(
            IItemRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHospitalCacheService hospitalCache,
            IMessageProducer messageProducer,
            IPositionService positionService
        )
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _hospitalCache = hospitalCache;
            _messageProducer = messageProducer;
            _positionService = positionService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            // 当传入的DetailData不为空时，则表示是批量增加
            if (saveModel.DetailData != null && saveModel.DetailData.Count > 0)
            {
                return BatchAdd(saveModel);
            }

            // 强制设置当前用户的机构和科室
            saveModel.MainData["OrganizationId"] = UserContext.Current.OrganizationId;
            saveModel.MainData["DepartmentId"] = UserContext.Current.DepartmentId;

            return base.Add(saveModel);
        }

        private WebResponseContent BatchAdd(SaveModel saveModel)
        {
            System.Collections.Generic.List<string> errorMessages = new System.Collections.Generic.List<string>();
            var addedItems = new List<(Item item, Dictionary<string, object> itemData)>();
            int successCount = 0;
            int totalCount = saveModel.DetailData.Count;
            string name = "";
            var orgId = UserContext.Current.OrganizationId;
            var deptId = UserContext.Current.DepartmentId;

            if (orgId <= 0 || deptId <= 0)
            {
                return new WebResponseContent().Error("当前用户未绑定有效机构或科室");
            }

            foreach (var itemData in saveModel.DetailData)
            {
                // 去除所有字段的收尾空白字符
                var keys = new List<string>(itemData.Keys);
                foreach (var key in keys)
                {
                    if (itemData[key] is string strValue && !string.IsNullOrEmpty(strValue))
                    {
                        itemData[key] = strValue.Trim();
                    }
                }

                string spec = itemData.GetValueOrDefault("Specification")?.ToString() ?? "";
                name = itemData.GetValueOrDefault("Name")?.ToString() ?? "";
                
                // 克隆原始数据，防止 base.Add 过滤掉非实体字段（如 Layer, LayerNO, ColumnNO）
                var originalItemData = new Dictionary<string, object>(itemData);

                try
                {
                    // 构造单个SaveModel进行添加
                    var singleModel = new SaveModel
                    {
                        MainData = itemData,
                        DetailData = null
                    };

                    // 1. 强制设置 OrganizationId 和 DepartmentId
                    itemData["OrganizationId"] = orgId;
                    itemData["DepartmentId"] = deptId;
                    singleModel.MainData["OrganizationId"] = orgId;
                    singleModel.MainData["DepartmentId"] = deptId;


                    // 2. 检查是否存在
                    // 使用(x.Specification ?? "") == spec 来同时处理NULL和空字符串的情况，确保兼容性
                    // 注意：Specfication字段如果不为null，也需要与spec进行比较

                    // 使用细粒度锁防止并发重复添加
                    string key = $"Item_Add_{orgId}_{deptId}_{name}_{spec}";
                    WebResponseContent? result = null;
                    bool exists = false;
                    Item? currentItem = null;

                    AddOnExecuting = (item, detail) =>
                    {
                        currentItem = item;
                        return new WebResponseContent(true);
                    };

                    KeyLock.Lock(key, () =>
                    {
                        exists = _repository.Exists(x => x.OrganizationId == orgId
                                                         && x.DepartmentId == deptId
                                                         && x.Name == name
                                                         && (x.Specification ?? "") == spec);

                        if (!exists)
                        {
                            // 调用基类Add方法，复用现有逻辑和验证
                            singleModel.MainData["AvailableStock"] = 0;
                            var cardinality = 0;
                            if (itemData.TryGetValue("Cardinality", out var value))
                            {
                                cardinality = value.GetInt();
                            }
                            if (cardinality <= 0)
                            {
                                singleModel.MainData["Cardinality"] = 1;
                            }
                            result = base.Add(singleModel);
                        }
                    });

                    if (exists)
                    {
                        errorMessages.Add($"物品[{name} {spec}]添加失败: 已存在相同名称和规格的物品");
                        continue;
                    }

                    if (result != null)
                    {
                        if (result.Status)
                        {
                            successCount++;
                            if (currentItem != null)
                            {
                                addedItems.Add((currentItem, originalItemData));
                            }
                        }
                        else
                        {
                            errorMessages.Add($"物品[{name} {spec}]添加失败: {result.Message}");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    errorMessages.Add($"物品[{name} {spec}]异常: {ex.Message}");
                }
            }

            // 批量添加成功后，判断第一个成功添加的物品是否满足 Layer 条件，满足则进行后续绑定操作
            if (addedItems.Count > 0 && IsSyncEnabled(addedItems[0].itemData))
            {
                SyncItemPositions(addedItems, orgId, deptId);
            }

            string message = $"批量处理完成。总数: {totalCount}, 成功: {successCount}, 失败: {errorMessages.Count}。\r\n";
            if (errorMessages.Count > 0)
            {
                message += " 错误详情: \r\n" + string.Join(";\r\n ", errorMessages);
                return new WebResponseContent().Error(message);
            }

            return new WebResponseContent(true).OK(message);
        }

        public async Task<WebResponseContent> CheckItemWarningsAsync(long id)
        {
            var db = _repository.DbContext;
            List<Item> items = new List<Item>();
            Dictionary<long, List<string>> carMapping = new Dictionary<long, List<string>>();

            if (id == 0)
            {
                // 一次查询获取所有绑定到抢救车的物品及其对应的抢救车名称映射
                var queryData = (from p in db.Set<Position>()
                                 join w in db.Set<Position>() on p.WarehouseId equals w.Id
                                 join i in db.Set<Item>() on p.ItemId equals i.Id
                                 where p.Category == 110 && p.IsDeleted != true && i.IsDeleted != true
                                 select new { Item = i, CarName = w.DisplayName })
                                .ToList();

                items = queryData.GroupBy(x => x.Item.Id).Select(g => g.First().Item).ToList();
                carMapping = queryData.GroupBy(x => x.Item.Id)
                                      .ToDictionary(g => g.Key, g => g.Select(x => x.CarName).Distinct().ToList());
            }
            else
            {
                var queryData = (from i in db.Set<Item>()
                                 join p in db.Set<Position>() on i.Id equals p.ItemId into ps
                                 from p in ps.DefaultIfEmpty()
                                 join w in db.Set<Position>() on p.WarehouseId equals w.Id into ws
                                 from w in ws.DefaultIfEmpty()
                                 where i.Id == id && i.IsDeleted != true && (p == null || (p.Category == 110 && p.IsDeleted != true))
                                 select new { Item = i, CarName = w.DisplayName })
                                .ToList();

                if (!queryData.Any()) return new WebResponseContent().Error("物品不存在");

                items.Add(queryData[0].Item);
                carMapping[id] = queryData.Where(x => x.CarName != null).Select(x => x.CarName).Distinct().ToList();
            }

            if (items.Count == 0) return new WebResponseContent(true);

            var now = DateTime.Now;
            var today = now.Date;

            // 用于缓存每个科室的订阅者，避免在批量模式下重复查询
            var subscriberCache = new Dictionary<(long?, long?), List<string>>();

            foreach (var item in items)
            {
                // 1. 获取通知接收者 (当前部门 Level 20, 21, 22)
                var deptKey = (item.OrganizationId, item.DepartmentId);
                if (!subscriberCache.TryGetValue(deptKey, out List<string>? clientIds))
                {
                    var targetSubscriberIds = db.Set<OrganizationUser>()
                        .Where(x => x.OrganizationId == item.OrganizationId
                                    && x.DepartmentId == item.DepartmentId
                                    && (x.Level == 20 || x.Level == 21 || x.Level == 22)
                                    && x.IsDeleted != true)
                        .Select(x => x.UserId ?? 0)
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();

                    clientIds = new List<string>();
                    foreach (var uid in targetSubscriberIds)
                    {
                        var account = _hospitalCache.GetUserAccount<UserAccount>(uid, 10);
                        if (account != null && !string.IsNullOrEmpty(account.Account))
                        {
                            clientIds!.Add(account.Account);
                        }
                    }
                    subscriberCache[deptKey] = clientIds;
                }

                // 2. 组装抢救车信息前缀
                carMapping.TryGetValue(item.Id, out List<string>? itemCarNames);
                string carInfo = (itemCarNames != null && itemCarNames.Any()) ? $"[{string.Join(",", itemCarNames)}]" : "";

                // 3. 库存不足检测 (Category 40)
                if (item.AvailableStock < item.Cardinality)
                {
                    if (!db.Set<WarningLogs>().Any(x => x.Category == 40 && x.RelatedId == item.Id && x.IsFinished == false))
                    {
                        string desc = $"{carInfo}[{item.Name}]库存不足(当前:{item.AvailableStock}, 基数:{item.Cardinality})";
                        db.Set<WarningLogs>().Add(new WarningLogs
                        {
                            Id = YitIdHelper.NextId(),
                            Category = 40,
                            Description = desc,
                            OrganizationId = item.OrganizationId,
                            DepartmentId = item.DepartmentId,
                            RelatedId = item.Id,
                            CreationTime = now,
                            IsFinished = false
                        });

                        if (clientIds.Count > 0)
                        {
                            var payload = new { Id = item.Id, ItemId = item.Id, Name = item.Name, Stock = item.AvailableStock };
                            // 发布到消息队列异步处理 (Phase 3)
                            var msg = new NotificationMessage
                            {
                                ClientIds = clientIds,
                                Title = "警告",
                                Content = desc,
                                Payload = payload
                            };
                            await _messageProducer.PublishAsync(msg, Define.ExchangeName, Define.NotificationQueue);
                        }
                    }
                }

                // 4. 效期检测 (Category 30)
                var details = db.Set<ItemDetail>().Where(x => x.ItemId == item.Id && x.IsDeleted != true && x.Stock > 0).ToList();
                foreach (var detail in details)
                {
                    string prefix = $"{carInfo}[{item.Name}][批次:{detail.RegisterCode}]";
                    string? desc = null;
                    string title = "警告";

                    if (detail.ExpirationDate < today)
                    {
                        desc = $"{prefix}已过期(效期:{detail.ExpirationDate:yyyy-MM-dd})";
                        title = "异常";
                    }
                    else if (detail.ExpirationDate < today.AddDays(30)) desc = $"{prefix}30天内到期(效期:{detail.ExpirationDate:yyyy-MM-dd})";
                    else if (detail.ExpirationDate < today.AddDays(60)) desc = $"{prefix}60天内到期(效期:{detail.ExpirationDate:yyyy-MM-dd})";
                    else if (detail.ExpirationDate < today.AddDays(90)) desc = $"{prefix}90天内到期(效期:{detail.ExpirationDate:yyyy-MM-dd})";

                    if (desc != null)
                    {
                        if (!db.Set<WarningLogs>().Any(x => x.Category == 30 && x.RelatedId == detail.Id && x.IsFinished == false))
                        {
                            db.Set<WarningLogs>().Add(new WarningLogs
                            {
                                Id = YitIdHelper.NextId(),
                                Category = 30,
                                Description = desc,
                                OrganizationId = item.OrganizationId,
                                DepartmentId = item.DepartmentId,
                                RelatedId = detail.Id,
                                CreationTime = now,
                                IsFinished = false
                            });

                            if (clientIds.Count > 0)
                            {
                                var payload = new { Id = detail.Id, RegisterCode = detail.RegisterCode, Name = item.Name, ItemId = item.Id, ExpirationDate = detail.ExpirationDate, Stock = detail.Stock };
                                // 发布到消息队列异步处理 (Phase 3)
                                var msg = new NotificationMessage
                                {
                                    ClientIds = clientIds,
                                    Title = title,
                                    Content = desc,
                                    Payload = payload
                                };
                                await _messageProducer.PublishAsync(msg, Define.ExchangeName, Define.NotificationQueue);
                            }
                        }
                    }
                }
            }

            db.SaveChanges();
            return new WebResponseContent(true);
        }

        public async Task<WebResponseContent> GetItemRemarkAsync(string combinedId)
        {
            if (string.IsNullOrEmpty(combinedId)) return new WebResponseContent(true);

            // 解析 DepartmentId+Barcode
            // 格式示例: "123+A001"
            var parts = combinedId.Split('+');
            if (parts.Length < 2) return new WebResponseContent(true);

            if (!long.TryParse(parts[0], out long departmentId))
            {
                return new WebResponseContent(true);
            }

            // Barcode 可能是剩余部分的所有内容（如果Barcode本身包含+虽然不常见但以防万一）
            // 这里假设只是两部分，或者取剩下所有的
            string barcode = combinedId.Substring(parts[0].Length + 1);

            if (string.IsNullOrEmpty(barcode)) return new WebResponseContent(true);

            string key = $"Item_Remark_{departmentId}_{barcode}";
            // 注意：这里暂时保留原始缓存逻辑，或者考虑在 IHospitalCacheService 中添加方法
            if (_hospitalCache.GetUserAccount<string>(0, 0) != null) // Placeholder for more logic
            {
               // TODO
            }

            var item = await _repository.FindFirstAsync(x =>
                x.DepartmentId == departmentId && x.Barcode == barcode && x.IsDeleted != true);

            _hospitalCache.Set(key, item);
            return new WebResponseContent(true).OK(data:new
            {
                Remark = item?.Remark,
                Name = item?.Name,
                AvailableStock = item?.AvailableStock,
                LatestExpirationDate = item?.LatestExpirationDate,
                Unit = item?.Unit,
                Form = item?.Form,
            });
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

            UpdateOnExecuted = (item, obj, x, x1) =>
            {
                if (item != null && !string.IsNullOrEmpty(item.Barcode))
                {
                    string key = $"Item_Remark_{item.DepartmentId}_{item.Barcode}";
                    _hospitalCache.Remove(key);
                }

                return new WebResponseContent(true);
            };
            return base.Update(saveModel);
        }

        private bool IsSyncEnabled(Dictionary<string, object> data)
        {
            if (data == null) return false;
            // 兼容性判断：优先判断 Layer 字段 (通常是总层数)，其次判断 LayerNO
            object? layerVal = null;
            if (data.TryGetValue("Layer", out layerVal) && layerVal != null) { }
            else if (data.TryGetValue("LayerNO", out layerVal) && layerVal != null) { }
            
            if (layerVal == null) return false;

            int layerNum = layerVal.ToString().GetInt();
            // 放宽限制，1-10层都认为是开启了同步功能
            return layerNum >= 1 && layerNum <= 10;
        }

        private void SyncItemPositions(List<(Item item, Dictionary<string, object> itemData)> addedItems, long orgId, long deptId)
        {
            if (addedItems == null || addedItems.Count == 0) return;

            // 兜底：如果传入的 ID 为 0，尝试从第一个物品中恢复
            if (orgId <= 0) orgId = addedItems[0].item.OrganizationId ?? 0;
            if (deptId <= 0) deptId = addedItems[0].item.DepartmentId;

            if (orgId <= 0 || deptId <= 0) return;

            var db = _repository.DbContext;

            // 1. 获取或创建抢救车 (Category 10)
            var car = db.Set<Position>().FirstOrDefault(x => x.OrganizationId == orgId && x.DepartmentId == deptId && x.Category == 10 && x.DisplayName == "抢救车" && x.IsDeleted != true);

            // 获取第一个物品的 Layer 作为预期的总层数
            int totalLayers = 5;
            if (addedItems[0].itemData.TryGetValue("Layer", out object? totalLayerObj) && totalLayerObj != null)
            {
                totalLayers = totalLayerObj.ToString().GetInt();
                if (totalLayers <= 0) totalLayers = 5;
            }

            // 2. 按 LayerNO 分组处理具体的格位绑定
            var itemsByLayer = new Dictionary<int, List<(Item item, int col)>>();
            foreach (var pair in addedItems)
            {
                // 使用 LayerNO 决定具体的层索引
                if (pair.itemData.TryGetValue("LayerNO", out object? lNoObj) && lNoObj != null)
                {
                    int lNum = lNoObj.ToString().GetInt();
                    int cNum = 0;
                    if (pair.itemData.ContainsKey("ColumnNO")) cNum = pair.itemData["ColumnNO"].ToString().GetInt();

                    if (lNum > 0 && cNum > 0)
                    {
                        if (!itemsByLayer.ContainsKey(lNum)) itemsByLayer[lNum] = new List<(Item, int)>();
                        itemsByLayer[lNum].Add((pair.item, cNum));
                    }
                }
            }

            // 3. 如果车不存在，直接在当前 DbContext 中创建车和所有层级（避免跨 DbContext 事务问题）
            if (car == null)
            {
                // 创建抢救车
                car = new Position
                {
                    Id = YitIdHelper.NextId(),
                    OrganizationId = orgId,
                    DepartmentId = deptId,
                    Category = 10,
                    DisplayName = "抢救车",
                    FloorId = totalLayers,
                    OrderBy = 1,
                    CreationTime = DateTime.Now,
                    Creator = UserContext.Current.UserTrueName,
                    CreatorId = UserContext.Current.UserId
                };
                db.Set<Position>().Add(car);
                db.SaveChanges(); // 先保存车，获取 ID

                // 为每一层创建层级记录，并设置正确的 SerialId
                for (int layerIdx = 1; layerIdx <= totalLayers; layerIdx++)
                {
                    int maxColumnInBatch = itemsByLayer.ContainsKey(layerIdx) ? itemsByLayer[layerIdx].Max(x => x.col) : 0;
                    
                    var layerPos = new Position
                    {
                        Id = YitIdHelper.NextId(),
                        OrganizationId = orgId,
                        DepartmentId = deptId,
                        WarehouseId = car.Id,
                        Category = 100,
                        FloorId = layerIdx,
                        SerialId = maxColumnInBatch, // 直接设置正确的分隔总数
                        DisplayName = $"第{layerIdx}层",
                        OrderBy = layerIdx,
                        CreationTime = DateTime.Now,
                        Creator = UserContext.Current.UserTrueName,
                        CreatorId = UserContext.Current.UserId
                    };
                    db.Set<Position>().Add(layerPos);
                }
                db.SaveChanges(); // 保存所有层级
            }

            if (car == null || car.Id <= 0) return;

            // 4. 遍历所有层级，更新 SerialId 并创建格位
            for (int layerIdx = 1; layerIdx <= totalLayers; layerIdx++)
            {
                // 查找层级记录 (Category 100)
                var layerPos = db.Set<Position>().FirstOrDefault(x => x.WarehouseId == car.Id && x.Category == 100 && x.FloorId == layerIdx && x.IsDeleted != true);
                if (layerPos == null) continue; // 如果层不存在则跳过
                
                // 计算本次批次中该层的最大列号(ColumnNO)
                int maxColumnInBatch = itemsByLayer.ContainsKey(layerIdx) ? itemsByLayer[layerIdx].Max(x => x.col) : 0;

                // 更新 SerialId（取最大值）
                long currentMax = layerPos.SerialId ?? 0;
                if (maxColumnInBatch > currentMax)
                {
                    layerPos.SerialId = maxColumnInBatch;
                    db.SaveChanges(); // 立即保存更新
                }

                // 5. 补齐格位并绑定物品 (Category 110)
                var existingSteps = db.Set<Position>()
                    .Where(x => x.WarehouseId == car.Id && x.FloorId == layerPos.Id && x.Category == 110 && x.IsDeleted != true)
                    .ToList();

                var itemMap = itemsByLayer.ContainsKey(layerIdx) ? itemsByLayer[layerIdx].ToDictionary(k => k.col, v => v.item) : new Dictionary<int, Item>();

                // 如果该层已有设定总数或本次有新格位需求
                int totalCols = (int)(layerPos.SerialId ?? 0);
                for (int i = 1; i <= totalCols; i++)
                {
                    var step = existingSteps.FirstOrDefault(x => x.SerialId == i);
                    bool isNew = false;
                    if (step == null)
                    {
                        step = new Position
                        {
                            Id = YitIdHelper.NextId(),
                            OrganizationId = orgId,
                            DepartmentId = deptId,
                            WarehouseId = car.Id,
                            FloorId = layerPos.Id,
                            Category = 110,
                            SerialId = i,
                            DisplayName = $"第{i}格",
                            OrderBy = (layerIdx * 100) + (int)(i * 10),
                            CreationTime = DateTime.Now,
                            Creator = UserContext.Current.UserTrueName,
                            CreatorId = UserContext.Current.UserId
                        };
                        isNew = true;
                    }

                    if (itemMap.ContainsKey(i))
                    {
                        var boundItem = itemMap[i];
                        step.ItemId = boundItem.Id;
                        step.ItemName = boundItem.Name;
                    }

                    if (isNew) db.Set<Position>().Add(step);
                }
                db.SaveChanges();
            }
        }
    }
}
