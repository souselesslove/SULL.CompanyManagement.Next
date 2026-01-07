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
using System.Collections.Generic;
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
using Yitter.IdGenerator;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationDepartmentService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationDepartmentRepository _repository;//访问数据库
        private readonly IOrganizationUserRepository _organizationUserRepository;
        private readonly IOrganizationRepository _organizationRepository;


        private readonly IRegisterCodeRepository _registerCodeRepository;
        private readonly IDashboardService _dashboardService;

        [ActivatorUtilitiesConstructor]
        public OrganizationDepartmentService(
            IOrganizationDepartmentRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, 
            IOrganizationUserRepository organizationUserRepository, 
            IOrganizationRepository organizationRepository, 
            IRegisterCodeRepository registerCodeRepository,
            IDashboardService dashboardService)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationUserRepository = organizationUserRepository;
            _organizationRepository = organizationRepository;
            _repository = dbRepository;

            _registerCodeRepository = registerCodeRepository;
            _dashboardService = dashboardService;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            long orgId = UserContext.Current.OrganizationId;
            if (orgId <= 0)
            {
                return new WebResponseContent().Error("请先选择或加入一个机构");
            }
            string? name = saveModel.MainData["Name"]?.ToString();
            
            base.AddOnExecute = model =>
            {
                var exist = _repository.FindFirst(x => x.OrganizationId == orgId && x.Name == name && x.IsDeleted != true);
                return exist == null ? new WebResponseContent(true) : new WebResponseContent().Error($"在本机构中部门名称[{name}]已存在");
            };

            base.AddOnExecuting = (department, o) =>
            {
                department.OrganizationId = orgId;
                department.UserCounter = 1;
                // 默认将创建者设为部门负责人
                department.DutyUserId = UserContext.Current.UserId;
                department.DutyUser = UserContext.Current.UserTrueName;
                return new WebResponseContent(true);
            };

            base.AddOnExecuted = (dept, obj) =>
            {
                var user = _organizationUserRepository.FindFirst(x => x.UserId == UserContext.Current.UserId);
                if (user != null)
                {
                    long? oldDeptId = user.DepartmentId;
                    
                    // 1. 将当前人员加入到该部门
                    user.DepartmentId = dept.Id;
                    user.LastModifierId = UserContext.Current.UserId;
                    user.LastModifier = UserContext.Current.UserName;
                    user.LastModificationTime = DateTime.Now;
                    _organizationUserRepository.Update(user, true);

                    // 2. 如果人员之前属于其他部门，更新旧部门的人数
                    if (oldDeptId.HasValue && oldDeptId.Value > 0 && oldDeptId.Value != dept.Id)
                    {
                        _ = this.UpdateUserCounterAsync(oldDeptId.Value, -1);
                    }
                }

                // 3. 更新机构的部门数量
                if (dept.OrganizationId > 0)
                {
                    var org = _organizationRepository.FindFirst(x => x.Id == dept.OrganizationId);
                    if (org != null)
                    {
                        org.DepartmentCounter = Math.Max(0, (org.DepartmentCounter ?? 0) + 1);
                        org.LastModifierId = UserContext.Current.UserId;
                        org.LastModifier = UserContext.Current.UserTrueName;
                        org.LastModificationTime = DateTime.Now;
                        _organizationRepository.Update(org, true);
                    }
                }

                // 4. 记录日志
                if (dept.OrganizationId > 0)
                {
                    _dashboardService.AddLog(dept.OrganizationId.Value, "系统消息", $"系统消息：部门[{dept.Name}]由 {UserContext.Current.UserTrueName} 创建成功");
                }

                // 5. 刷新缓存
                UserContext.Current.LogOut(UserContext.Current.UserId);

                return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
            };

            // 使用细粒度锁防止重复添加部门
            string key = $"OrganizationDepartment_Add_{orgId}_{name}";
            return KeyLock.Lock(key, () =>
            {
                return base.Add(saveModel);
            });
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            var depts = new List<OrganizationDepartment>();
            if (keys != null && keys.Length > 0)
            {
                var longKeys = keys.Select(x => x.ToString().GetLong()).ToList();
                depts = _repository.Find(x => longKeys.Contains(x.Id)).ToList();
            }

            DelOnExecuted = (k) =>
            {
                foreach (var dept in depts)
                {
                    if (dept.OrganizationId > 0)
                    {
                        var org = _organizationRepository.FindFirst(x => x.Id == dept.OrganizationId);
                        if (org != null)
                        {
                            org.DepartmentCounter = Math.Max(0, (org.DepartmentCounter ?? 0) - 1);
                            org.LastModifierId = UserContext.Current.UserId;
                            org.LastModifier = UserContext.Current.UserTrueName;
                            org.LastModificationTime = DateTime.Now;
                            _organizationRepository.Update(org, true);
                        }
                    }
                }
                return new WebResponseContent(true);
            };
            return base.Del(keys, delList);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            // check validates
            if (saveModel.MainData.ContainsKey("DutyUserId") || saveModel.MainData.ContainsKey("DutyUser"))
            {
                if (saveModel.MainData.TryGetValue("Id", out object? idObj))
                {
                    long id = idObj.ToString().GetLong();
                    var entity = _repository.FindFirst(x => x.Id == id);
                    if (entity != null)
                    {
                        // 检查值是否真正发生改变
                        bool isChanged = false;
                        if (saveModel.MainData.TryGetValue("DutyUserId", out object? newIdObj))
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

            UpdateOnExecuted = (department, obj, list, list2) =>
            {
                // 如果修改涉及到了某些关键信息（虽然部门修改通常不改变当前人的部门，但如果是后台管理修改了人员部门，建议刷新）
                // 考虑到这里是部门服务，如果是用户切换了部门，通常是在 OrganizationUserService 处理
                // 但为了保险，如果当前用户操作了部门相关的更新，触一次刷新也是安全的
                UserContext.Current.LogOut(UserContext.Current.UserId);
                return new WebResponseContent(true);
            };

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
                && a.IsDeleted != true
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
                  && a.IsDeleted != true
                select a;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public virtual async Task UpdateUserCounterAsync(long deptId, int delta)
        {
            var dept = _repository.FindFirst(x => x.Id == deptId);
            if (dept != null)
            {
                dept.UserCounter = Math.Max(0, (dept.UserCounter ?? 0) + delta);
                dept.LastModifierId = UserContext.Current.UserId;
                dept.LastModifier = UserContext.Current.UserTrueName;
                dept.LastModificationTime = DateTime.Now;
                _repository.Update(dept, true);
            }
            await Task.CompletedTask;
        }
    }
}
