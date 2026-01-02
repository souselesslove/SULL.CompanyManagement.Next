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
using System.Collections.Generic;
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
using System.Threading;
using System.Threading.Tasks;
using System;
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationUserRepository _repository; // 访问数据库
        private readonly IOrganizationDepartmentRepository _organizationDepartmentRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IDashboardService _dashboardService;
        private readonly ICacheService _cache;
        private readonly IUniPushService _uniPushService;

        [ActivatorUtilitiesConstructor]
        public OrganizationUserService(
            IOrganizationUserRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, 
            IOrganizationDepartmentRepository organizationDepartmentRepository, 
            IOrganizationRepository organizationRepository,
            IUserAccountRepository userAccountRepository,
            IDashboardService dashboardService,
            ICacheService cache,
            IUniPushService uniPushService)
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationDepartmentRepository = organizationDepartmentRepository;
            _organizationRepository = organizationRepository;
            _userAccountRepository = userAccountRepository;
            _repository = dbRepository;
            _dashboardService = dashboardService;
            _cache = cache;
            _uniPushService = uniPushService;
        }

        public virtual async Task<UserOrganizationDto> GetUserOrganizationAsync(CancellationToken cancellationToken = default)
        {
            var ret = new UserOrganizationDto();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            ret.User = await _repository.FindFirstAsync(x => x.UserId == userId && x.IsDeleted != true);
            if (ret.User == null)
            {
                return ret;
            }
            ret.Organization = await _organizationRepository.FindFirstAsync(x => x.Id == ret.User.OrganizationId && x.IsDeleted != true);
            ret.Department = await _organizationDepartmentRepository.FindFirstAsync(x => x.Id == ret.User.DepartmentId && x.IsDeleted != true);
            return ret;
        }

        public override PageGridData<OrganizationUser> GetPageData(PageDataOptions options)
        {
            // 仅能获取当前机构的人员信息
            long orgId = UserContext.Current.OrganizationId;
            if (orgId > 0)
            {
                QueryRelativeExpression = (query) =>
                {
                    return query.Where(x => x.OrganizationId == orgId);
                };
            }
            return base.GetPageData(options);
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

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuted = (user, response) =>
            {
                // 清理职级缓存
                _cache.Remove($"UserLevel_{user.UserId}");

                // 1. Update Org UserCounter
                if (user.OrganizationId > 0)
                {
                    var org = _organizationRepository.FindFirst(x => x.Id == user.OrganizationId);
                    if (org != null)
                    {
                        org.UserCounter = (org.UserCounter ?? 0) + 1;
                        org.LastModifierId = UserContext.Current.UserId;
                        org.LastModifier = UserContext.Current.UserTrueName;
                        org.LastModificationTime = DateTime.Now;
                        _organizationRepository.Update(org, true);
                    }
                }
                // 2. Update Dept UserCounter
                if (user.DepartmentId > 0)
                {
                    var dept = _organizationDepartmentRepository.FindFirst(x => x.Id == user.DepartmentId);
                    if (dept != null)
                    {
                        dept.UserCounter = (dept.UserCounter ?? 0) + 1;
                        dept.LastModifierId = UserContext.Current.UserId;
                        dept.LastModifier = UserContext.Current.UserTrueName;
                        dept.LastModificationTime = DateTime.Now;
                        _organizationDepartmentRepository.Update(dept, true);
                        
                        // 推送消息给部门责任人
                        if (dept.DutyUserId > 0)
                        {
                            _ = NotifyDepartmentHeadAsync(dept, $"{user.UserName} 加入了部门[{dept.Name}]");
                        }
                    }
                }
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            var users = new List<OrganizationUser>();
            if (keys != null && keys.Length > 0)
            {
                var longKeys = keys.Select(x => x.ToString().GetLong()).ToList();
                users = _repository.Find(x => longKeys.Contains(x.Id)).ToList();
            }

            DelOnExecuted = (k) =>
            {
                foreach (var user in users)
                {
                    // 清理职级缓存
                    _cache.Remove($"UserLevel_{user.UserId}");

                    // 1. Update Org UserCounter
                    if (user.OrganizationId > 0)
                    {
                        var org = _organizationRepository.FindFirst(x => x.Id == user.OrganizationId);
                        if (org != null)
                        {
                            org.UserCounter = Math.Max(0, (org.UserCounter ?? 0) - 1);
                            org.LastModifierId = UserContext.Current.UserId;
                            org.LastModifier = UserContext.Current.UserTrueName;
                            org.LastModificationTime = DateTime.Now;
                            _organizationRepository.Update(org, true);
                        }
                    }
                    // 2. Update Dept UserCounter
                    if (user.DepartmentId > 0)
                    {
                        var dept = _organizationDepartmentRepository.FindFirst(x => x.Id == user.DepartmentId);
                        if (dept != null)
                        {
                            dept.UserCounter = Math.Max(0, (dept.UserCounter ?? 0) - 1);
                            dept.LastModifierId = UserContext.Current.UserId;
                            dept.LastModifier = UserContext.Current.UserTrueName;
                            dept.LastModificationTime = DateTime.Now;
                            _organizationDepartmentRepository.Update(dept, true);

                            // 推送消息给部门责任人
                            if (dept.DutyUserId > 0)
                            {
                                _ = NotifyDepartmentHeadAsync(dept, $"{user.UserName} 退出了部门[{dept.Name}]");
                            }
                        }
                    }
                }
                return new WebResponseContent(true);
            };
            return base.Del(keys, delList);
        }

        public override WebResponseContent Update(SaveModel saveDataModel)
        {
            if (!saveDataModel.MainData.ContainsKey("Id")) return base.Update(saveDataModel);

            long id = saveDataModel.MainData["Id"].ToString().GetLong();
            var oldUser = _repository.FindFirst(x => x.Id == id);
            long? oldOrgId = oldUser?.OrganizationId;
            long? oldDeptId = oldUser?.DepartmentId;

            UpdateOnExecuting = (user, obj, x1, x2) =>
            {
                if (oldUser == null) return new WebResponseContent(true);

                // 0. Auto-map Title based on Level
                var levelTitleMap = new Dictionary<int, string>
                {
                    { 60, "院长" },
                    { 61, "书记" },
                    { 50, "分管院长" },
                    { 40, "护理部主任" },
                    { 31, "科主任" },
                    { 30, "护士长" },
                    { 22, "医生" },
                    { 21, "技师" },
                    { 20, "护士" },
                    { 11, "护工" },
                    { 10, "后勤" }
                };

                // Auto-set Title or Validate Level
                if (user.Level != null)
                {
                    if (levelTitleMap.TryGetValue(user.Level.Value, out string title))
                    {
                        user.Title = title;
                    }
                    else if (user.Level != oldUser.Level)
                    {
                        return new WebResponseContent().Error("非法操作");
                    }
                }

                // Check if Level or Title is being modified
                if (user.Level != oldUser.Level || user.Title != oldUser.Title)
                {
                    // If Super Admin, bypass checks
                    if (UserContext.Current.IsSuperAdmin) return new WebResponseContent(true);

                    var currentUserId = UserContext.Current.UserId;
                    var currentUserRecord = _repository.FindFirst(x => x.UserId == currentUserId && x.IsDeleted != true);
                    
                    int currentUserLevel = currentUserRecord?.Level ?? 0;
                    int newLevel = user.Level ?? 0;
                    
                    // Requirement: Current User Level > Modified Level
                    if (currentUserLevel <= newLevel)
                    {
                         return new WebResponseContent().Error("非法操作");
                    }
                }
                
                return new WebResponseContent(true);
            };

            UpdateOnExecuted = (user, obj, x, x1) =>
            {
                // 清理职级缓存
                _cache.Remove($"UserLevel_{user.UserId}");

                // 1. Handle Organization Change
                if (user.OrganizationId != oldOrgId)
                {
                    if (oldOrgId > 0)
                    {
                        var oldOrg = _organizationRepository.FindFirst(x => (long?)x.Id == oldOrgId);
                        if (oldOrg != null)
                        {
                            oldOrg.UserCounter = Math.Max(0, (oldOrg.UserCounter ?? 0) - 1);
                            oldOrg.LastModifierId = UserContext.Current.UserId;
                            oldOrg.LastModifier = UserContext.Current.UserTrueName;
                            oldOrg.LastModificationTime = DateTime.Now;
                            _organizationRepository.Update(oldOrg, true);
                        }
                    }
                    if (user.OrganizationId > 0)
                    {
                        var newOrg = _organizationRepository.FindFirst(x => x.Id == user.OrganizationId);
                        if (newOrg != null)
                        {
                            newOrg.UserCounter = (newOrg.UserCounter ?? 0) + 1;
                            newOrg.LastModifierId = UserContext.Current.UserId;
                            newOrg.LastModifier = UserContext.Current.UserTrueName;
                            newOrg.LastModificationTime = DateTime.Now;
                            _organizationRepository.Update(newOrg, true);
                        }
                    }
                }

                // 2. Handle Department Change
                if (user.DepartmentId != oldDeptId)
                {
                    // Log Leaving Old Dept
                    if (oldDeptId > 0)
                    {
                        var oldDept = _organizationDepartmentRepository.FindFirst(x => (long?)x.Id == oldDeptId);
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

                                // 推送消息给部门责任人
                                if (oldDept.DutyUserId > 0)
                                {
                                    _ = NotifyDepartmentHeadAsync(oldDept, $"{user.UserName} 已退出部门[{oldDept.Name}]");
                                }
                            }
                        }
                    }

                    // Log Joining New Dept
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

                                // 推送消息给部门责任人
                                if (newDept.DutyUserId > 0)
                                {
                                    _ = NotifyDepartmentHeadAsync(newDept, $"{user.UserName} 已加入部门[{newDept.Name}]");
                                }
                            }
                        }
                    }
                }
                return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
            };
            return base.Update(saveDataModel);
        }

        private async Task NotifyDepartmentHeadAsync(OrganizationDepartment dept, string content)
        {
            if (dept.DutyUserId == null || dept.DutyUserId <= 0) return;

            string accountCacheKey = $"UserAccount_{dept.DutyUserId}_10_Account";
            string headAccount = null;

            // 1. 尝试从缓存获取
            var cachedAccount = _cache.Get<UserAccount>(accountCacheKey);
            if (cachedAccount != null)
            {
                headAccount = cachedAccount.Account;
            }
            else
            {
                // 2. 查库
                var account = await _userAccountRepository.DbContext.Set<UserAccount>()
                    .FirstOrDefaultAsync(x => x.UserId == dept.DutyUserId && x.Category == 10 && x.IsDeleted != true);
                if (account != null)
                {
                    headAccount = account.Account;
                    _cache.AddObject(accountCacheKey, account);
                }
            }

            if (!string.IsNullOrEmpty(headAccount))
            {
                await _uniPushService.SendAsync(new List<string> { headAccount }, "部门成员变动通知", content);
            }
        }
    }
}
