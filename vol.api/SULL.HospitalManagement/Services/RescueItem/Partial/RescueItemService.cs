/*
 *所有关于RescueItem类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*RescueItemService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace SULL.HospitalManagement.Services
{
    public partial class RescueItemService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRescueItemRepository _repository;//访问数据库

        [ActivatorUtilitiesConstructor]
        public RescueItemService(
            IRescueItemRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override PageGridData<RescueItem> GetPageData(PageDataOptions options)
        {
            QueryRelativeExpression = (IQueryable<RescueItem> query) =>
            {
                var db = _repository.DbContext;
                var q = from ri in query
                        join r in db.Set<Rescue>() on ri.RescueId equals r.Id
                        where r.OrganizationId == UserContext.Current.OrganizationId 
                           && r.DepartmentId == UserContext.Current.DepartmentId
                        select ri;
                return q;
            };
            return base.GetPageData(options);
        }
  }
}
