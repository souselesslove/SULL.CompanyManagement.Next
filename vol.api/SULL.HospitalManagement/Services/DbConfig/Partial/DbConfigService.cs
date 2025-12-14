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
using Microsoft.AspNetCore.Http;
using SULL.HospitalManagement.IRepositories;
using VOL.Core.Enums;
using VOL.Core.ManageUser;

namespace SULL.HospitalManagement.Services
{
    public partial class DbConfigService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDbConfigRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public DbConfigService(
            IDbConfigRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<DbConfig> GetPageData(PageDataOptions options)
        {
            options = ValidatePageOptions(options, out IQueryable<DbConfig> queryable);
            //获取排序字段
            Dictionary<string, QueryOrderBy> orderbyDic = GetPageDataSort(options, typeof(DbConfig).GetProperties());

            var pageGridData = new PageGridData<DbConfig>();
            if (QueryRelativeExpression != null)
            {
                queryable = QueryRelativeExpression.Invoke(queryable);
            }
            var userId = UserContext.Current.UserId;
            queryable = queryable.Where(x => x.Category == "G" || x.Category == "O" || x.Category == "D" || (x.Category == "U" && x.RelatedId == userId));
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
                //查询界面统计求等字段
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
    }
}
