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
using VOL.Core.RabbitMQ;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SULL.HospitalManagement.Repositories;
using VOL.Core.Enums;
using SULL.HospitalManagement.IServices;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationUserRepository _repository;//访问数据库
        private readonly IOrganizationDepartmentRepository _organizationDepartmentRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IDashboardService _dashboardService;

        [ActivatorUtilitiesConstructor]
        public OrganizationUserService(
            IOrganizationUserRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IOrganizationDepartmentRepository organizationDepartmentRepository, IOrganizationRepository organizationRepository,
            IDashboardService dashboardService)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationDepartmentRepository = organizationDepartmentRepository;
            _organizationRepository = organizationRepository;
            _repository = dbRepository;
            _dashboardService = dashboardService;
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
                where a.UserId == userId && b.IsDeleted != true
                select b;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public override WebResponseContent Update(SaveModel saveDataModel)
        {
            if (!saveDataModel.MainData.ContainsKey("Id")) return base.Update(saveDataModel);

            long id = saveDataModel.MainData["Id"].ToString().GetLong();
            var oldUser = _repository.FindFirst(x => x.Id == id);
            long? oldDeptId = oldUser?.DepartmentId;

            base.UpdateOnExecuted = (user, obj, x, x1) =>
            {
                // Check if Department changed
                if (user.DepartmentId != oldDeptId)
                {
                    // 1. Log Leaving Old Dept
                    if (oldDeptId > 0)
                    {
                        var oldDept = _organizationDepartmentRepository.FindFirst(x => x.Id == oldDeptId);
                        if (oldDept != null)
                        {
                            // Update Counter
                            oldDept.UserCounter = Math.Max(0, (oldDept.UserCounter ?? 0) - 1);
                            oldDept.LastModifierId = UserContext.Current.UserId;
                            oldDept.LastModifier = UserContext.Current.UserTrueName;
                            oldDept.LastModificationTime = DateTime.Now;
                            _organizationDepartmentRepository.Update(oldDept, true);

                            // Log
                            if (oldDept.OrganizationId > 0)
                            {
                                string msg = $"系统消息：由 {UserContext.Current.UserTrueName} 将用户[{user.UserName}]移出部门[{oldDept.Name}]";
                                _dashboardService.AddLog(oldDept.OrganizationId.Value, "系统消息", msg);
                            }
                        }
                    }

                    // 2. Log Joining New Dept
                    if (user.DepartmentId > 0)
                    {
                        var newDept = _organizationDepartmentRepository.FindFirst(x => x.Id == user.DepartmentId);
                        if (newDept != null)
                        {
                            // Update Counter
                            newDept.UserCounter = (newDept.UserCounter ?? 0) + 1;
                            newDept.LastModifierId = UserContext.Current.UserId;
                            newDept.LastModifier = UserContext.Current.UserTrueName;
                            newDept.LastModificationTime = DateTime.Now;
                            _organizationDepartmentRepository.Update(newDept, true);

                            // Log
                            if (newDept.OrganizationId > 0)
                            {
                                string msg = $"系统消息：由 {UserContext.Current.UserTrueName} 将用户[{user.UserName}]加入部门[{newDept.Name}]";
                                _dashboardService.AddLog(newDept.OrganizationId.Value, "系统消息", msg);
                            }
                        }
                    }
                }
                return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
            };
            return base.Update(saveDataModel);
        }
    }
}
