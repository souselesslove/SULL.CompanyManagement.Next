/*
 *所有关于OrganizationDepartment类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OrganizationDepartmentService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using VOL.Core.Enums;
using VOL.Core.ManageUser;
using SULL.HospitalManagement.IServices;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationDepartmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationDepartmentRepository _repository;//访问数据库
        private readonly IOrganizationUserRepository _organizationUserRepository;
        private readonly IOrganizationRepository _organizationRepository;

        private readonly IOrganizationUserService _organizationUserService;
        private readonly IRegisterCodeRepository _registerCodeRepository;
        private readonly IDashboardService _dashboardService;

        [ActivatorUtilitiesConstructor]
        public OrganizationDepartmentService(
            IOrganizationDepartmentRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, IOrganizationUserRepository organizationUserRepository, IOrganizationRepository organizationRepository, IOrganizationUserService organizationUserService, IRegisterCodeRepository registerCodeRepository,
            IDashboardService dashboardService)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationUserRepository = organizationUserRepository;
            _organizationRepository = organizationRepository;
            _repository = dbRepository;
            _organizationUserService = organizationUserService;
            _registerCodeRepository = registerCodeRepository;
            _dashboardService = dashboardService;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            base.AddOnExecute = model =>
            {
                //校验邀请码
                if (!saveModel.MainData.ContainsKey("Code") || string.IsNullOrEmpty(saveModel.MainData["Code"]?.ToString()))
                {
                    return new WebResponseContent().Error("请输入邀请码");
                }
                string code = saveModel.MainData["Code"].ToString();
                var registerCode = _registerCodeRepository.FindFirst(x => x.Code == code);
                if (registerCode == null)
                {
                    return new WebResponseContent().Error("邀请码不存在");
                }
                if (registerCode.RelatedId != null || registerCode.UseTime != null)
                {
                    return new WebResponseContent().Error("邀请码已被使用");
                }

                var exist = _repository.FindFirst(x => x.Name == saveModel.MainData["Name"].ToString());
                return exist == null ? new WebResponseContent(true) : new WebResponseContent().Error("部门已存在");
            };
            var user = _organizationUserRepository.FindFirst(x => x.UserId == UserContext.Current.UserId);
            base.AddOnExecuting = (department, o) =>
            {
                department.UserCounter = 1;
                department.OrganizationId = user.OrganizationId;
                return new WebResponseContent(true);
            };
            base.AddOnExecuted = (organization, obj) =>
            {
                user.DepartmentId = organization.Id;
                user.LastModifierId = UserContext.Current.UserId;
                user.LastModifier = UserContext.Current.UserName;
                user.LastModificationTime = DateTime.Now;
                _organizationUserRepository.Update(user, true);

                var o = _organizationRepository.FindFirst(x => x.Id == user.OrganizationId);
                o.DepartmentCounter = o.DepartmentCounter + 1;
                o.LastModifierId = UserContext.Current.UserId;
                o.LastModifier = UserContext.Current.UserName;
                o.LastModificationTime = DateTime.Now;
                _organizationRepository.Update(o, true);

                //更新邀请码使用状态
                if (saveModel.MainData.ContainsKey("Code"))
                {
                    string code = saveModel.MainData["Code"].ToString();
                    var registerCode = _registerCodeRepository.FindFirst(x => x.Code == code);
                    if (registerCode != null)
                    {
                        registerCode.RelatedId = (int)organization.Id;
                        registerCode.UseTime = DateTime.Now;
                        _registerCodeRepository.Update(registerCode, true);
                    }
                }

                // Log to Dashboard
                if (organization.OrganizationId > 0)
                {
                     _dashboardService.AddLog(organization.OrganizationId.Value, "系统消息", $"系统消息：部门[{organization.Name}]由 {UserContext.Current.UserTrueName} 创建成功");
                }

                return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
            };
            // 使用细粒度锁防止重复添加部门
            string key = $"OrganizationDepartment_Add_{saveModel.MainData["Name"]}";
            return KeyLock.Lock(key, () =>
            {
                return base.Add(saveModel);
            });
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            // check validates
            if (saveModel.MainData.ContainsKey("DutyUserId") || saveModel.MainData.ContainsKey("DutyUser"))
            {
                object idObj = null;
                if (saveModel.MainData.TryGetValue("Id", out idObj))
                {
                    long id = idObj.ToString().GetLong();
                    var entity = _repository.FindFirst(x => x.Id == id);
                    if (entity != null)
                    {
                        // 检查值是否真正发生改变
                        bool isChanged = false;
                        if (saveModel.MainData.TryGetValue("DutyUserId", out object newIdObj))
                        {
                            int? newId = newIdObj == null || string.IsNullOrEmpty(newIdObj.ToString()) ? (int?)null : newIdObj.ToString().GetInt();
                            if (entity.DutyUserId != newId) isChanged = true;
                        }
                        
                        // 如果值改变了，检查权限
                        if (isChanged)
                        {
                            int currentUserId = UserContext.Current.UserId;
                            // 只有创建人 或 原负责人 可以修改
                            if (currentUserId != entity.CreatorId && currentUserId != entity.DutyUserId)
                            {
                                return new WebResponseContent().Error("非法操作");
                            }
                        }
                    }
                }
            }
            return base.Update(saveModel);
        }

        public virtual async Task<PageGridData<OrganizationDepartment>> GetUserDepartmentAsync(CancellationToken cancellationToken = default)
        {
            var pageGridData = new PageGridData<OrganizationDepartment>();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            var qry = from a in db.Set<OrganizationDepartment>()
                join b in db.Set<OrganizationUser>() on a.Id equals b.DepartmentId
                where b.UserId == userId
                && b.IsDeleted != true
                select a;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public virtual async Task<PageGridData<OrganizationDepartment>> GetOrganizationDepartmentAsync(CancellationToken cancellationToken = default)
        {
            var pageGridData = new PageGridData<OrganizationDepartment>();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            var qry = from a in db.Set<OrganizationDepartment>()
            join c in db.Set<Organization>() on a.OrganizationId equals c.Id
            join b in db.Set<OrganizationUser>() on c.Id equals b.OrganizationId
            where b.UserId == userId
                  && b.IsDeleted != true
                select a;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }
    }
}
