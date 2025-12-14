/*
 *所有关于Position类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*PositionService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.CacheManager;
using System.Collections.Generic;
using Newtonsoft.Json;
using Yitter.IdGenerator;

namespace SULL.HospitalManagement.Services
{
    public partial class PositionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPositionRepository _repository;//访问数据库
        private readonly ICacheService _cache;
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public PositionService(
            IPositionRepository dbRepository,
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

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuting = (position, response) =>
            {
                if (position.OrganizationId <= 0 || position.DepartmentId <= 0)
                {
                    return new WebResponseContent().Error("非法请求");
                }

                if (position.Category == 10)
                {
                    if (_repository.Exists(x => x.OrganizationId == position.OrganizationId
                        && x.DepartmentId == position.DepartmentId
                        && x.Category == 10
                        && x.DisplayName == position.DisplayName))
                    {
                        return new WebResponseContent().Error($"抢救车[{position.DisplayName}]已存在，请勿重复添加");
                    }
                }
                return new WebResponseContent(true);
            };

            // 当入参包含Category=10时，表示是添加抢救车，添加完抢救车后，还需要根据入参的Floor层数，添加对应的每一层信息
            if (saveModel.MainData.TryGetValue("Category", out object category) && category?.ToString() == "10")
            {
                var floor = saveModel.MainData["FloorId"].GetInt();
                AddOnExecuted = (position, response) =>
                {
                    if (floor > 0)
                    {
                        var floorList = new List<Position>();

                        for (int i = 1; i <= floor; i++)
                        {
                            var floorPos = new Position
                            {
                                Id = YitIdHelper.NextId(),
                                OrganizationId = position.OrganizationId,
                                DepartmentId = position.DepartmentId,
                                WarehouseId = position.Id,
                                ToolId = 0,
                                FloorId = i, // 设置层号
                                Category = 100, // 100代表层级
                                SerialId = 0, // 当Category=100时，SerialId代表当前分层的分隔数量
                                DisplayName = $"第{i}层",
                                OrderBy = i,
                                Creator = UserContext.Current.UserTrueName,
                                CreatorId = UserContext.Current.UserId,
                                CreationTime = DateTime.Now
                            };
                            floorList.Add(floorPos);
                        }

                        if (floorList.Count > 0)
                        {
                            _repository.AddRange(floorList, true);
                        }
                    }
                    return new WebResponseContent(true);
                };
            }
            if (saveModel.MainData.TryGetValue("Category", out object catObj) && catObj?.ToString() == "10")
            {
                // 使用细粒度锁防止重复添加抢救车
                string key = $"Position_Add_10_{saveModel.MainData["OrganizationId"]}_{saveModel.MainData["DepartmentId"]}_{saveModel.MainData["DisplayName"]}";
                return KeyLock.Lock(key, () =>
                {
                    return base.Add(saveModel);
                });
            }
            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            // 当更新的内容是分层时Category=100，需要检测SerialId(分隔数量)的变化
            if (saveModel.MainData.TryGetValue("Id", out object idObj) && saveModel.MainData.TryGetValue("SerialId", out object serialIdObj))
            {
                if (long.TryParse(idObj.ToString(), out long id))
                {
                    var entity = _repository.FindFirst(x => x.Id == id);
                    // 只有当Category=100且SerialId确实发生变化时才处理
                    if (entity != null && entity.Category == 100)
                    {
                        long oldSerialId = entity.SerialId ?? 0;
                        long newSerialId = serialIdObj.GetInt();
                        int orderStart = entity.OrderBy;
                        if (newSerialId > oldSerialId)
                        {
                            // 数量增加：将超过部分SerialId依次新增到数据库（分隔的Category=110）
                            var addList = new List<Position>();
                            for (long i = oldSerialId + 1; i <= newSerialId; i++)
                            {
                                orderStart += 10;
                                addList.Add(new Position
                                {
                                    Id = YitIdHelper.NextId(),
                                    OrganizationId = entity.OrganizationId,
                                    DepartmentId = entity.DepartmentId,
                                    WarehouseId = entity.WarehouseId,
                                    FloorId = entity.Id,
                                    Category = 110, // 分隔
                                    SerialId = i,
                                    DisplayName = $"第{i}格",
                                    OrderBy = orderStart,
                                    CreationTime = DateTime.Now,
                                    Creator = UserContext.Current.UserTrueName,
                                    CreatorId = UserContext.Current.UserId
                                });
                            }
                            if (addList.Count > 0)
                            {
                                _repository.AddRange(addList);
                            }
                        }
                        else if (newSerialId < oldSerialId)
                        {
                            // 数量减少：将旧的SerialId多余的依次设置IsDeleted=1
                            var delList = _repository.Find(x => x.WarehouseId == entity.WarehouseId
                                                             && x.FloorId == entity.Id
                                                             && x.Category == 110
                                                             && x.SerialId > newSerialId);
                            if (delList.Count > 0)
                            {
                                delList.ForEach(x =>
                                {
                                    x.IsDeleted = true;
                                    x.DeleterId = UserContext.Current.UserId;
                                    x.Deleter = UserContext.Current.UserTrueName;
                                    x.DeletionTime = DateTime.Now;
                                });
                                _repository.UpdateRange(delList);
                            }
                        }
                    }
                }
            }
            return base.Update(saveModel);
        }
        public override PageGridData<Position> GetPageData(PageDataOptions options)
        {
            // IsDeleted 默认需要添加条件IsDeleted!=1
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
                        return new PageGridData<Position> { rows = new List<Position>(), total = 0 };
                    }

                    // 异常检测: 使用缓存记录用户编号及查询的OrganizationId
                    var userId = UserContext.Current.UserId;
                    string cacheKey = $"PositionQuery_Org_{userId}";
                    
                    var queriedOrgs = _cache.Get<HashSet<long>>(cacheKey);

                    if (queriedOrgs == null)
                    {
                        queriedOrgs = new HashSet<long>();
                    }

                    if (!queriedOrgs.Contains(orgId))
                    {
                        queriedOrgs.Add(orgId);
                        // Cache for reasonable time (e.g. 1 hour = 3600 seconds)
                        _cache.AddObject(cacheKey, queriedOrgs, 3600);

                        // 如果同一个用户编号查询了三个不同的OrganizationId
                        if (queriedOrgs.Count == 3)
                        {
                            // 向管理员发布一条异常消息（TodoList的Category=30）
                            var todo = new SaveModel
                            {
                                MainData = new Dictionary<string, object>
                                {
                                    { "Category", 30 },
                                    { "Description", $"异常查询：用户[{UserContext.Current.UserTrueName}]查询了超过3个不同组织机构的位置数据" },
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
        
  }
}
