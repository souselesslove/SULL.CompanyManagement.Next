/*
 *所有关于Organization类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*OrganizationService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SULL.HospitalManagement.IServices;
using Yitter.IdGenerator;
using VOL.Core.Enums;
using VOL.Core.CacheManager;
using SULL.HospitalManagement.IServices.Dtos;
using VOL.Core.RabbitMQ;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SULL.HospitalManagement.Services
{
    public partial class OrganizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationRepository _repository; //访问数据库
        private readonly IOrganizationUserRepository _organizationUserRepository;
        private readonly IOrganizationDepartmentRepository _organizationDepartmentRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IDashboardService _dashboardService;
        private readonly IHospitalCacheService _hospitalCache;
        private readonly IMessageProducer _messageProducer;

        [ActivatorUtilitiesConstructor]
        public OrganizationService(
            IOrganizationRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, 
            IOrganizationUserRepository organizationUserRepository,
            IOrganizationDepartmentRepository organizationDepartmentRepository,
            IUserAccountRepository userAccountRepository,
            IDashboardService dashboardService,
            IHospitalCacheService hospitalCache,
            IMessageProducer messageProducer)
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationUserRepository = organizationUserRepository;
            _organizationDepartmentRepository = organizationDepartmentRepository;
            _userAccountRepository = userAccountRepository;
            _dashboardService = dashboardService;
            _repository = dbRepository;
            _hospitalCache = hospitalCache;
            _messageProducer = messageProducer;
        }

        public virtual async Task<PageGridData<Organization>> GetUserOrganizationAsync(
            CancellationToken cancellationToken = default)
        {
            var pageGridData = new PageGridData<Organization>();
            var userId = UserContext.Current.UserId;
            var db = _repository.DbContext;
            var qry = from a in db.Set<Organization>()
                join b in db.Set<OrganizationUser>() on a.Id equals b.OrganizationId
                where b.UserId == userId && a.IsDeleted != true && b.IsDeleted != true
                select a;
            pageGridData.total = await qry.CountAsync(cancellationToken);
            pageGridData.rows = await qry.ToListAsync(cancellationToken);
            return pageGridData;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            saveModel.MainData.TryGetValue("Name", out object? nameObj);
            saveModel.MainData.TryGetValue("Level", out object? levelObj);
            saveModel.MainData.TryGetValue("InvitationCode", out object? codeObj);

            string? name = nameObj?.ToString();
            string? level = levelObj?.ToString();
            string? code = codeObj?.ToString();

            // 1. 如果提供了邀请码，走加入机构流程
            if (!string.IsNullOrWhiteSpace(code))
            {
                var org = _repository.FindFirst(x => x.InvitationCode == code);
                if (org == null) return new WebResponseContent().Error("邀请码不正确");

                // 检查用户是否已在该机构
                if (_organizationUserRepository.Exists(x => x.OrganizationId == org.Id && x.UserId == UserContext.Current.UserId && x.IsDeleted != true))
                {
                    return new WebResponseContent().Error("您已在该机构中，请勿重复加入");
                }

                org.UserCounter = (org.UserCounter ?? 0) + 1;
                org.LastModifierId = UserContext.Current.UserId;
                org.LastModifier = UserContext.Current.UserTrueName;
                org.LastModificationTime = DateTime.Now;
                _repository.Update(org, true);

                var ou = new OrganizationUser
                {
                    Id = YitIdHelper.NextId(),
                    UserId = UserContext.Current.UserId,
                    OrganizationId = org.Id,
                    UserName = UserContext.Current.UserTrueName,
                    Category = 0,
                    Level = 1,
                    AuditStatus = 1,
                    CreationTime = DateTime.Now,
                    CreatorId = UserContext.Current.UserId,
                    Creator = UserContext.Current.UserTrueName
                };
                _organizationUserRepository.Add(ou, true);

                _dashboardService.AddLog(org.Id, "系统消息", $"系统消息：{UserContext.Current.UserTrueName} 加入了机构[{org.Name}]");
                
                // 刷新缓存
                UserContext.Current.LogOut(UserContext.Current.UserId);
                
                return new WebResponseContent(true).OK("加入成功");
            }

            // 2. 如果提供了名称和级别，走创建/激活流程
            if (!string.IsNullOrWhiteSpace(name))
            {
                var existingOrg = _repository.FindFirst(x => x.Name == name);
                if (existingOrg != null)
                {
                    // 机构已存在且人数为0，则直接将人加到机构，然后更新机构信息
                    if ((existingOrg.UserCounter ?? 0) <= 0)
                    {
                        existingOrg.Level = level;
                        existingOrg.UserCounter = 1;
                        // 重新设置邀请码为Id
                        existingOrg.InvitationCode = existingOrg.Id.ToString();
                        existingOrg.AuditStatus = 1;
                        existingOrg.LastModifierId = UserContext.Current.UserId;
                        existingOrg.LastModifier = UserContext.Current.UserTrueName;
                        existingOrg.LastModificationTime = DateTime.Now;
                        _repository.Update(existingOrg, true);

                        var ou = new OrganizationUser
                        {
                            Id = YitIdHelper.NextId(),
                            UserId = UserContext.Current.UserId,
                            OrganizationId = existingOrg.Id,
                            UserName = UserContext.Current.UserTrueName,
                            Category = 0,
                            Level = 40,
                            Title = "护理部主任",
                            AuditStatus = 1,
                            CreationTime = DateTime.Now,
                            CreatorId = UserContext.Current.UserId,
                            Creator = UserContext.Current.UserTrueName
                        };
                        _organizationUserRepository.Add(ou, true);

                        _dashboardService.AddLog(existingOrg.Id, "系统消息", $"系统消息：机构[{existingOrg.Name}]由 {UserContext.Current.UserTrueName} 重新激活并接管");
                        
                        // 刷新缓存
                        UserContext.Current.LogOut(UserContext.Current.UserId);
                        
                        return new WebResponseContent(true).OK("机构已激活并接管成功");
                    }
                    else
                    {
                        return new WebResponseContent().Error("该机构名称已存在");
                    }
                }

                // 定义新增 Hook
                base.AddOnExecuting = (org, o) =>
                {
                    org.UserCounter = 1;
                    // 新增时，需要将机构的邀请码直接设为机构的编号Id
                    org.InvitationCode = org.Id.ToString();
                    org.AuditStatus = 0;
                    return new WebResponseContent(true);
                };

                base.AddOnExecuted = (org, obj) =>
                {
                    var ou = new OrganizationUser
                    {
                        Id = YitIdHelper.NextId(),
                        UserId = UserContext.Current.UserId,
                        OrganizationId = org.Id,
                        UserName = UserContext.Current.UserTrueName,
                        Category = 0,
                        Level = 40,
                        Title = "护理部主任",
                        AuditStatus = 1,
                        CreationTime = DateTime.Now,
                        CreatorId = UserContext.Current.UserId,
                        Creator = UserContext.Current.UserTrueName
                    };
                    _organizationUserRepository.Add(ou, true);

                    _dashboardService.AddLog(org.Id, "系统消息", $"系统消息：机构[{org.Name}]由 {UserContext.Current.UserTrueName} 创建成功");
                    
                    // 刷新缓存
                    UserContext.Current.LogOut(UserContext.Current.UserId);
                    
                    return new WebResponseContent(true).OK(ResponseType.SaveSuccess);
                };

                string lockKey = $"Organization_Add_{name}";
                return KeyLock.Lock(lockKey, () => base.Add(saveModel));
            }

            return new WebResponseContent().Error("请提供机构名称和级别，或提供有效的邀请码");
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            var user = UserContext.Current;
            // 1. 如果是超级管理员，走基类的逻辑（真正的机构删除/禁用）
            if (user.IsSuperAdmin)
            {
                return base.Del(keys, delList);
            }

            // 2. 对于普通用户，Del 行为被重定义为“退出机构”
            var orgIds = keys.Select(x => x.ToString().GetLong()).ToList();
            var userId = user.UserId;

            // 查找这些机构中当前用户的成员记录
            var memberships = _organizationUserRepository.Find(x => 
                orgIds.Contains(x.OrganizationId ?? 0) && 
                x.UserId == userId && 
                x.IsDeleted != true).ToList();

            if (!memberships.Any())
            {
                return new WebResponseContent().Error("未找到在该机构的成员记录");
            }

            foreach (var ou in memberships)
            {
                // 1. 软删除成员关系记录
                ou.IsDeleted = true;
                _organizationUserRepository.Update(ou, true);

                // 2. 更新机构的人数统计
                var org = _repository.FindFirst(x => x.Id == ou.OrganizationId);
                if (org != null)
                {
                    org.UserCounter = Math.Max(0, (org.UserCounter ?? 0) - 1);
                    _repository.Update(org, true);
                    _dashboardService.AddLog(org.Id, "系统消息", $"系统消息：{user.UserTrueName} 已退出该机构");

                    // 推送消息给机构创建人
                        if (org.CreatorId > 0 && org.CreatorId != userId)
                        {
                            // 优先从缓存获取
                            var accountEntity = _hospitalCache.GetUserAccount<UserAccount>(org.CreatorId.Value, 10);
                            string? creatorAccount = accountEntity?.Account;
                            
                            if (string.IsNullOrEmpty(creatorAccount))
                            {
                                // 缓存没有则查库
                                var account = _userAccountRepository.FindFirst(x => x.UserId == org.CreatorId && x.Category == 10);
                                if (account != null)
                                {
                                    creatorAccount = account.Account;
                                    // 写入缓存
                                    _hospitalCache.SetUserAccount(org.CreatorId.Value, 10, account);
                                }
                            }

                            if (!string.IsNullOrEmpty(creatorAccount))
                            {
                                // 发布到消息队列异步处理
                                var msg = new NotificationMessage
                                {
                                    ClientIds = new List<string> { creatorAccount },
                                    Title = "成员退出通知",
                                    Content = $"{user.UserTrueName} 已退出了机构[{org.Name}]"
                                };
                                _messageProducer.PublishAsync(msg, Define.ExchangeName, Define.NotificationQueue).GetAwaiter().GetResult();
                            }
                        }
                }

                // 3. 更新部门的人数统计 (如果用户在该机构有部门)
                if (ou.DepartmentId > 0)
                {
                    var dept = _organizationDepartmentRepository.FindFirst(x => x.Id == ou.DepartmentId);
                    if (dept != null)
                    {
                        dept.UserCounter = Math.Max(0, (dept.UserCounter ?? 0) - 1);
                        dept.LastModifierId = UserContext.Current.UserId;
                        dept.LastModifier = UserContext.Current.UserTrueName;
                        dept.LastModificationTime = DateTime.Now;
                        _organizationDepartmentRepository.Update(dept, true);
                    }
                }

                // 4. 清除职级权限缓存
                _hospitalCache.RemoveUserLevel(userId);
            }

            // 5. 强制登出以刷新用户的本地缓存及权限状态
            user.LogOut(userId);

            return new WebResponseContent(true).OK("已成功退出所选机构");
        }

        public virtual async Task UpdateUserCounterAsync(long orgId, int delta)
        {
            var org = _repository.FindFirst(x => x.Id == orgId);
            if (org != null)
            {
                org.UserCounter = Math.Max(0, (org.UserCounter ?? 0) + delta);
                org.LastModifierId = UserContext.Current.UserId;
                org.LastModifier = UserContext.Current.UserTrueName;
                org.LastModificationTime = DateTime.Now;
                _repository.Update(org, true);
            }
            await Task.CompletedTask;
        }

        public virtual async Task UpdateDepartmentCounterAsync(long orgId, int delta)
        {
            var org = _repository.FindFirst(x => x.Id == orgId);
            if (org != null)
            {
                org.DepartmentCounter = Math.Max(0, (org.DepartmentCounter ?? 0) + delta);
                org.LastModifierId = UserContext.Current.UserId;
                org.LastModifier = UserContext.Current.UserTrueName;
                org.LastModificationTime = DateTime.Now;
                _repository.Update(org, true);
            }
            await Task.CompletedTask;
        }
    }
}
