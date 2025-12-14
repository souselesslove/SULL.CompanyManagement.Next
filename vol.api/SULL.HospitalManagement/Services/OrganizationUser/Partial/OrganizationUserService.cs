/*
 *所有关于OrganizationUser类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OrganizationUserService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using SULL.HospitalManagement.IServices.Dtos;
using VOL.Core.ManageUser;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SULL.HospitalManagement.Repositories;
using VOL.Core.Enums;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationUserRepository _repository;//访问数据库
        private readonly IOrganizationDepartmentRepository _organizationDepartmentRepository;
        private readonly IOrganizationRepository _organizationRepository;

        [ActivatorUtilitiesConstructor]
        public OrganizationUserService(
            IOrganizationUserRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IOrganizationDepartmentRepository organizationDepartmentRepository, IOrganizationRepository organizationRepository)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationDepartmentRepository = organizationDepartmentRepository;
            _organizationRepository = organizationRepository;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public virtual async Task<UserOrganizationDto> GetUserOrganizationAsync(CancellationToken cancellationToken = default)
        {
            var ret = new UserOrganizationDto();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            ret.User = await _repository.FindFirstAsync(x => x.UserId == userId);
            if (ret.User == null)
            {
                return ret;
            }
            ret.Organization = await _organizationRepository.FindFirstAsync(x => x.Id == ret.User.OrganizationId);
            ret.Department = await _organizationDepartmentRepository.FindFirstAsync(x => x.Id == ret.User.DepartmentId);
            return ret;
        }

        public virtual async Task<PageGridData<OrganizationUser>> GetDepartmentUserAsync(CancellationToken cancellationToken = default)
        {
            var pageGridData = new PageGridData<OrganizationUser>();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            var qry = from a in db.Set<OrganizationUser>()
                join b in db.Set<OrganizationUser>() on a.DepartmentId equals b.DepartmentId
                where a.UserId == userId
                select b;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public override WebResponseContent Update(SaveModel saveDataModel)
        {
            var user = _repository.FindFirst(x => x.UserId == UserContext.Current.UserId);
            base.AddOnExecuted = (organization, obj) =>
            {
                var department = _organizationDepartmentRepository.FindFirst(x => x.Id == user.DepartmentId);
                department.UserCounter -= 1;
                department.LastModifierId = UserContext.Current.UserId;
                department.LastModifier = UserContext.Current.UserName;
                department.LastModificationTime = DateTime.Now;
                _organizationDepartmentRepository.Update(department, true);
                return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
            };
            return base.Update(saveDataModel);
        }
    }
}
