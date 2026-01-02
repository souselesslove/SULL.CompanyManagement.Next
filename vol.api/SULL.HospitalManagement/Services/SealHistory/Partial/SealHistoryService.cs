/*
 *所有关于SealHistory类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*SealHistoryService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using SULL.HospitalManagement.IServices;
using Yitter.IdGenerator;
using VOL.Core.CacheManager;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace SULL.HospitalManagement.Services
{
    public partial class SealHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISealHistoryRepository _repository;//访问数据库

        private readonly IMessageProducer _messageProducer;
        private readonly IDashboardService _dashboardService;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly ICacheService _cache;
        private readonly IUniPushService _uniPushService;

        [ActivatorUtilitiesConstructor]
        public SealHistoryService(
            ISealHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor, 
            IMessageProducer messageProducer,
            IDashboardService dashboardService,
            IUserAccountRepository userAccountRepository,
            ICacheService cache,
            IUniPushService uniPushService
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _messageProducer = messageProducer;
            _dashboardService = dashboardService;
            _userAccountRepository = userAccountRepository;
            _cache = cache;
            _uniPushService = uniPushService;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            saveModel.MainData.Add("Status", 0);
            base.AddOnExecuted = (sealHistory, response) =>
            {
                //新增成功后，给确认人发送一条确认消息
                if (sealHistory.AssignUserId != null)
                {
                    // 发送消息
                    var todo = new SaveModel();
                    todo.MainData = new Dictionary<string, object>();
                    todo.MainData.Add("RelatedId", sealHistory.Id);
                    todo.MainData.Add("Category", 40);
                    todo.MainData.Add("Description", $"{sealHistory.Creator}向您提交了电子封存申请");
                    todo.MainData.Add("LatestAssignUserId", sealHistory.AssignUserId);
                    todo.MainData.Add("LatestAssignUser", sealHistory.AssignUser);
                    todo.MainData.Add("Kind", 2);

                    var detail = new Dictionary<string, object>();
                    detail.Add("Description", "电子封存申请");
                    detail.Add("Status", 0);
                    todo.DetailData = new List<Dictionary<string, object>>();
                    todo.DetailData.Add(detail);

                    // TODO 后台线程无法获取用户信息
                    todo.MainData.Add("UserInfo", UserContext.Current.UserInfo);
                    todo.MainData.Add("IsSuperAdmin", UserContext.Current.IsSuperAdmin);
                    _messageProducer.PublishAsync(todo, Define.ExchangeName, Define.TodoQueue).GetAwaiter().GetResult();

                    // 发送推送通知
                    string actionType = sealHistory.Category == 10 ? "封存" : (sealHistory.Category == 20 ? "启封" : "申请");
                    string notifyTitle = $"{actionType}确认提醒";
                    string notifyContent = $"{sealHistory.Creator} 向您提交了电子{actionType}申请，请确认。";
                    var payload = new { Id = sealHistory.Id, Category = sealHistory.Category, RelatedId = sealHistory.RelatedId };
                    _ = NotifyUserAsync(sealHistory.AssignUserId.Value, notifyTitle, notifyContent, payload);
                }
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            if (saveModel.MainData.TryGetValue("Status", out object status) && status != null)
            {
                string statusStr = status.ToString();
                if (statusStr == "1")
                {
                    if (saveModel.MainData.TryGetValue("Id", out object id) && id != null && long.TryParse(id.ToString(), out long handoverId))
                    {
                        var entity = _repository.FindFirst(x => x.Id == handoverId);
                        if (entity == null)
                        {
                            return new WebResponseContent().Error("数据不存在");
                        }
                        if (entity.Status != 0)
                        {
                            return new WebResponseContent().Error("非法操作");
                        }

                        // Log to Dashboard
                        var db = _repository.DbContext;
                        var car = db.Set<Position>().FirstOrDefault(x => x.Id == entity.RelatedId);
                        if (car != null && car.OrganizationId > 0)
                        {
                            string deptName = "未知科室";
                            string carName = car.DisplayName;
                            string operatorName = UserContext.Current.UserTrueName;

                            if (car.DepartmentId > 0)
                            {
                                var dept = db.Set<OrganizationDepartment>().FirstOrDefault(x => x.Id == car.DepartmentId);
                                if (dept != null) deptName = dept.Name;
                            }
                            
                            string logMsg = $"[{deptName}]-[{carName}] 由 {operatorName} 完成封存";
                            _dashboardService.AddLog(car.OrganizationId.Value, "封存", logMsg);

                            // 向申请人推送通知
                            if (entity.CreatorId > 0 && entity.CreatorId != UserContext.Current.UserId)
                            {
                                string actionType = entity.Category == 10 ? "封存" : (entity.Category == 20 ? "启封" : "申请");
                                string notifyTitle = $"{actionType}完成通知";
                                string notifyContent = $"您提交的电子{actionType}申请已由 {operatorName} 确认完成。";
                                var notifyPayload = new { Id = entity.Id, Category = entity.Category, RelatedId = entity.RelatedId };
                                _ = NotifyUserAsync(entity.CreatorId.Value, notifyTitle, notifyContent, notifyPayload);
                            }

                            var endTime = DateTime.Now;
                            var duration = (int)(endTime - entity.CreationTime.Value).TotalSeconds;
                            var durationStat = new DurationStatistics
                            {
                                Id = YitIdHelper.NextId(),
                                OrganizationId = car.OrganizationId,
                                DepartmentId = car.DepartmentId,
                                Category = 20,
                                RelatedId = entity.Id,
                                CreatorId = UserContext.Current.UserId,
                                Creator = UserContext.Current.UserTrueName,
                                CreationTime = endTime,
                                StartTime = entity.CreationTime,
                                EndTime = endTime,
                                Duration = duration,
                                Remark = ""
                            };
                            db.Set<DurationStatistics>().Add(durationStat);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return base.Update(saveModel);
        }

        private async Task NotifyUserAsync(int userId, string title, string content, object payload = null)
        {
            if (userId <= 0) return;

            string accountCacheKey = $"UserAccount_{userId}_10_Account";
            string userAccount = null;

            // 1. 尝试从缓存获取
            var cachedAccount = _cache.Get<UserAccount>(accountCacheKey);
            if (cachedAccount != null)
            {
                userAccount = cachedAccount.Account;
            }
            else
            {
                // 2. 查库
                var account = await _userAccountRepository.DbContext.Set<UserAccount>()
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDeleted != true && x.Category == 10);
                if (account != null)
                {
                    userAccount = account.Account;
                    _cache.AddObject(accountCacheKey, account);
                }
            }

            if (!string.IsNullOrEmpty(userAccount))
            {
                await _uniPushService.SendAsync(new List<string> { userAccount }, title, content, payload);
            }
        }
    }
}
