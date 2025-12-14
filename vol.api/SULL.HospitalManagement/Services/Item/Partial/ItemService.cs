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
using SULL.HospitalManagement.IRepositories;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;
using VOL.Core.ManageUser;
using VOL.Core.RabbitMQ;
using VOL.Core.CacheManager;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SULL.HospitalManagement.Services
{
    public partial class ItemService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemRepository _repository;//访问数据库
        private readonly ICacheService _cache;
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public ItemService(
            IItemRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cache,
            IMessageProducer messageProducer
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _cache = cache;
            _messageProducer = messageProducer;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<Item> GetPageData(PageDataOptions options)
        {
            if(string.IsNullOrEmpty(options.Wheres)){
                return new PageGridData<Item> { rows = new List<Item>(), total = 0 };
            }
            // 解析查询条件
            if (!string.IsNullOrEmpty(options.Wheres))
            {
                try
                {
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

                    // 如果条件里缺少OrganizationId和DepartmentId，且均需要大于零
                    if (orgId <= 0 || deptId <= 0)
                    {
                        return new PageGridData<Item> { rows = new List<Item>(), total = 0 };
                    }

                    // 异常检测: 使用缓存记录用户编号及查询的OrganizationId
                    var userId = UserContext.Current.UserId;
                    string cacheKey = $"ItemQuery_Org_{userId}";
                    
                    var queriedOrgs = _cache.Get<HashSet<long>>(cacheKey);

                    if (queriedOrgs == null)
                    {
                        queriedOrgs = new HashSet<long>();
                    }

                    if (!queriedOrgs.Contains(orgId))
                    {
                        queriedOrgs.Add(orgId);
                        // Cache for reasonable time (e.g. 1 hour)
                        _cache.AddObject(cacheKey, queriedOrgs, 360000);

                        // 如果同一个用户编号查询了三个不同的OrganizationId
                        if (queriedOrgs.Count == 3)
                        {
                            // 向管理员发布一条异常消息（TodoList的Category=30）
                            var todo = new SaveModel
                            {
                                MainData = new Dictionary<string, object>
                                {
                                    { "Category", 30 },
                                    { "Description", $"异常查询：用户[{UserContext.Current.UserTrueName}]查询了超过3个不同组织机构的数据" },
                                    { "Remark", $"OrgIds: {string.Join(",", queriedOrgs)}" }
                                }
                            };
                            _messageProducer.PublishAsync(todo, Define.ExchangeName, Define.TodoQueue);
                        }
                    }
                }
                catch { }
            }

            return base.GetPageData(options);
        }
        public override WebResponseContent Add(SaveModel saveModel)
        {
            // 当传入的DetailData不为空时，则表示是批量增加
            if (saveModel.DetailData != null && saveModel.DetailData.Count > 0)
            {
                return BatchAdd(saveModel);
            }
            return base.Add(saveModel);
        }

        private WebResponseContent BatchAdd(SaveModel saveModel)
        {
            System.Collections.Generic.List<string> errorMessages = new System.Collections.Generic.List<string>();
            int successCount = 0;
            int totalCount = saveModel.DetailData.Count;
            string name = "";
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

                string spec = itemData.ContainsKey("Specification") && itemData["Specification"] != null ? itemData["Specification"].ToString() : "";
                name = itemData.ContainsKey("Name") && itemData["Name"] != null ? itemData["Name"].ToString() : "";
                try
                {
                    // 构造单个SaveModel进行添加
                    var singleModel = new SaveModel
                    {
                        MainData = itemData,
                        DetailData = null
                    };

                    
                    // 1. 校验 OrganizationId 和 DepartmentId
                    long orgId = 0;
                    long deptId = 0;
                    if (itemData.ContainsKey("OrganizationId") && itemData["OrganizationId"] != null)
                        long.TryParse(itemData["OrganizationId"].ToString(), out orgId);
                    
                    if (itemData.ContainsKey("DepartmentId") && itemData["DepartmentId"] != null)
                        long.TryParse(itemData["DepartmentId"].ToString(), out deptId);

                    if (orgId <= 0 || deptId <= 0)
                    {
                        errorMessages.Add($"物品[{name}]数据无效"); // 错误内容为“数据无效”
                        continue; 
                    }

                    // 2. 检查是否存在
                    // 使用(x.Specification ?? "") == spec 来同时处理NULL和空字符串的情况，确保兼容性
                    // 注意：Specfication字段如果不为null，也需要与spec进行比较

                    // 使用细粒度锁防止并发重复添加
                    string key = $"Item_Add_{orgId}_{deptId}_{name}_{spec}";
                    WebResponseContent result = null;
                    bool exists = false;

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
                            var cardinality = itemData.ContainsKey("Cardinality").GetInt();
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

            string message = $"批量处理完成。总数: {totalCount}, 成功: {successCount}, 失败: {errorMessages.Count}。\r\n";
            if (errorMessages.Count > 0)
            {
                message += " 错误详情: \r\n" + string.Join(";\r\n ", errorMessages);
                return new WebResponseContent().Error(message);
            }
            return new WebResponseContent(true).OK(message);
        }
  }
}
