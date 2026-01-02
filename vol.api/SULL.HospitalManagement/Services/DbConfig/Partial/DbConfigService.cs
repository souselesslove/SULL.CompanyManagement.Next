/*
 *所有关于DbConfig类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*DbConfigService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.Enums;
using VOL.Core.ManageUser;
using VOL.Core.CacheManager;
using Microsoft.AspNetCore.Http;
using SULL.HospitalManagement.IRepositories;

namespace SULL.HospitalManagement.Services
{
    public partial class DbConfigService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbConfigRepository _repository; //访问数据库
        private readonly ICacheService _cacheService;
        private const string CacheKey = "DB_CONFIG_CACHE";

        [ActivatorUtilitiesConstructor]
        public DbConfigService(
            IDbConfigRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cacheService
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _cacheService = cacheService;
        }

        private List<DbConfig> GetCacheConfigs()
        {
            var data = _cacheService.Get<List<DbConfig>>(CacheKey);
            if (data == null)
            {
                data = _repository.Find(x => x.IsDeleted != true);
                _cacheService.AddObject(CacheKey, data);
            }
            return data;
        }

        public override PageGridData<DbConfig> GetPageData(PageDataOptions options)
        {
            // Get all configs from cache
            var configs = GetCacheConfigs();

            // Filter by categories and user
            var userId = UserContext.Current.UserId;
            var queryable = configs.AsQueryable()
                .Where(x => x.Category == "G" || x.Category == "O" || x.Category == "D" || (x.Category == "U" && x.RelatedId == userId));

            // Apply search conditions from options
            options = ValidatePageOptions(options, out IQueryable<DbConfig> _);
            if (QueryRelativeExpression != null)
            {
                queryable = QueryRelativeExpression.Invoke(queryable);
            }

            // Get sorting info
            Dictionary<string, QueryOrderBy> orderbyDic = GetPageDataSort(options, typeof(DbConfig).GetProperties());

            var pageGridData = new PageGridData<DbConfig>();
            if (options.Export)
            {
                queryable = queryable.GetIQueryableOrderBy(orderbyDic);
                if (Limit > 0)
                {
                    queryable = queryable.Take(Limit);
                }
                pageGridData.rows = queryable.ToList();
            }
            else
            {
                if (SummaryExpress != null)
                {
                    pageGridData.summary = SummaryExpress.Invoke(queryable);
                }
                pageGridData.rows = repository.IQueryablePage(queryable,
                    options.Page,
                    options.Rows,
                    out int rowCount,
                    orderbyDic).ToList();
                pageGridData.total = rowCount;
            }
            GetPageDataOnExecuted?.Invoke(pageGridData);
            return pageGridData;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuted = (config, response) =>
            {
                _cacheService.Remove(CacheKey);
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            UpdateOnExecuted = (config, obj, x, x1) =>
            {
                _cacheService.Remove(CacheKey);
                return new WebResponseContent(true);
            };
            return base.Update(saveModel);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            DelOnExecuted = (k) =>
            {
                _cacheService.Remove(CacheKey);
                return new WebResponseContent(true);
            };
            return base.Del(keys, delList);
        }
    }
}
