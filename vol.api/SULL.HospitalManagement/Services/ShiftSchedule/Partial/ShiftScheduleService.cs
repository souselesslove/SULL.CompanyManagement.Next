/*
 *所有关于ShiftSchedule类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ShiftScheduleService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System.Collections.Generic;
using System;
using VOL.Core.ManageUser;
using VOL.Core.CacheManager;
using System.Threading.Tasks;

namespace SULL.HospitalManagement.Services
{
    public partial class ShiftScheduleService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShiftScheduleRepository _repository;//访问数据库
        private readonly ICacheService _cache;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IUniPushService _uniPushService;

        [ActivatorUtilitiesConstructor]
        public ShiftScheduleService(
            IShiftScheduleRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cache,
            IUserAccountRepository userAccountRepository,
            IUniPushService uniPushService
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _cache = cache;
            _userAccountRepository = userAccountRepository;
            _uniPushService = uniPushService;
        }

        public override PageGridData<ShiftSchedule> GetPageData(PageDataOptions options)
        {
            var user = UserContext.Current;
            var db = _repository.DbContext;

            // 获取当前用户的职级信息 (加入缓存逻辑)
            string cacheKey = $"UserLevel_{user.UserId}";
            int level = _cache.Get(cacheKey).GetInt();
            
            if (level == 0) // 由于 GetInt() 在找不到时返回 0，且 Level 通常 > 0 (如果是 -1 代表缓存过不存在)
            {
                var orgUserInfo = db.Set<OrganizationUser>()
                    .FirstOrDefault(x => x.UserId == user.UserId && x.IsDeleted != true);
                level = orgUserInfo?.Level ?? -1; 
                // 缓存一个月 (3600 * 24 * 30 秒)
                _cache.Add(cacheKey, level.ToString(), 3600 * 24 * 30); 
            }

            // 检测前端是否传递了时间范围（ScheduleDate）或特定人员（UserId）
            bool hasTimeFilter = false;
            long filterUserId = 0;
            if (!string.IsNullOrEmpty(options.Wheres))
            {
                var wheres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                if (wheres != null)
                {
                    hasTimeFilter = wheres.Any(x => x.Name == "ScheduleDate" && !string.IsNullOrEmpty(x.Value));
                    var userParam = wheres.FirstOrDefault(x => x.Name == "UserId" && !string.IsNullOrEmpty(x.Value));
                    if (userParam != null)
                    {
                        filterUserId = userParam.Value.GetLong();
                    }
                }
            }

            QueryRelativeExpression = (query) =>
            {
                var q = query;
                // 1. 权限过滤：超级管理员或 Level >= 30 可以查看全科数据，否则只能看自己
                if (user.IsSuperAdmin || level >= 30)
                {
                    q = q.Where(x => x.OrganizationId == user.OrganizationId && x.DepartmentId == user.DepartmentId);
                    // 如果传了特定的 UserId，则进一步过滤（通常框架会自动处理 Wheres，但此处显式加入能确保数据归属正确性）
                    if (filterUserId > 0)
                    {
                        q = q.Where(x => x.UserId == filterUserId);
                    }
                }
                else
                {
                    q = q.Where(x => x.UserId == user.UserId);
                }

                // 2. 默认时间过滤：如果没有传时间，默认查询本月数据（防止全量加载导致 OOM）
                if (!hasTimeFilter)
                {
                    var now = DateTime.Now;
                    var start = new DateTime(now.Year, now.Month, 1);
                    var end = start.AddMonths(1);
                    q = q.Where(x => x.ScheduleDate >= start && x.ScheduleDate < end);
                }

                return q;
            };

            return base.GetPageData(options);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuted = (schedule, response) =>
            {
                if (schedule.UserId > 0)
                {
                    string monthStr = schedule.ScheduleDate.ToString("M月");
                    _ = NotifyUserAsync(schedule.UserId.Value, "排班更新通知", $"您的{monthStr}排班信息有更新，请及时确认，以免耽误正常工作。");
                }
                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            UpdateOnExecuted = (schedule, obj, x, x1) =>
            {
                if (schedule.UserId > 0)
                {
                    string monthStr = schedule.ScheduleDate.ToString("M月");
                    _ = NotifyUserAsync(schedule.UserId.Value, "排班更新通知", $"您的{monthStr}排班信息有更新，请及时确认，以免耽误正常工作。");
                }
                return new WebResponseContent(true);
            };
            return base.Update(saveModel);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            var schedules = new List<ShiftSchedule>();
            if (keys != null && keys.Length > 0)
            {
                var longKeys = keys.Select(x => x.ToString().GetLong()).ToList();
                schedules = _repository.Find(x => longKeys.Contains(x.Id)).ToList();
            }

            DelOnExecuted = (k) =>
            {
                foreach (var schedule in schedules)
                {
                    if (schedule.UserId > 0)
                    {
                        string monthStr = schedule.ScheduleDate.ToString("M月");
                        _ = NotifyUserAsync(schedule.UserId.Value, "排班取消通知", $"您的{monthStr}排班信息有更新（已取消），请及时确认，以免耽误正常工作。");
                    }
                }
                return new WebResponseContent(true);
            };
            return base.Del(keys, delList);
        }

        private async Task NotifyUserAsync(int userId, string title, string content)
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
                await _uniPushService.SendAsync(new List<string> { userAccount }, title, content);
            }
        }
    }
}
