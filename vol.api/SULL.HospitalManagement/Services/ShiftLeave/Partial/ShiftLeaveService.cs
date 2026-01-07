/*
 *所有关于ShiftLeave类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ShiftLeaveService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using System;
using System.Collections.Generic;
using SULL.HospitalManagement.IServices;
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public partial class ShiftLeaveService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShiftLeaveRepository _repository;//访问数据库
        private readonly IHospitalCacheService _hospitalCache;

        [ActivatorUtilitiesConstructor]
        public ShiftLeaveService(
            IShiftLeaveRepository dbRepository,
            IHttpContextAccessor httpContextAccessor,
            IHospitalCacheService hospitalCache
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _hospitalCache = hospitalCache;
        }

        public override PageGridData<ShiftLeave> GetPageData(PageDataOptions options)
        {
            var user = UserContext.Current;
            var db = _repository.DbContext;

            // 获取当前用户的职级信息 (加入缓存逻辑)
            int level = _hospitalCache.GetUserLevel(user.UserId);

            if (level == 0)
            {
                var orgUserInfo = db.Set<OrganizationUser>()
                    .FirstOrDefault(x => x.UserId == user.UserId && x.IsDeleted != true);
                level = orgUserInfo?.Level ?? -1;
                _hospitalCache.SetUserLevel(user.UserId, level);
            }

            // 检测前端是否传递了时间范围（StartDate/EndDate）或特定人员（CreatorId）
            bool hasTimeFilter = false;
            long filterCreatorId = 0;
            if (!string.IsNullOrEmpty(options.Wheres))
            {
                var wheres = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                if (wheres != null)
                {
                    hasTimeFilter = wheres.Any(x => (x.Name == "StartDate" || x.Name == "EndDate") && !string.IsNullOrEmpty(x.Value));
                    var creatorParam = wheres.FirstOrDefault(x => x.Name == "CreatorId" && !string.IsNullOrEmpty(x.Value));
                    if (creatorParam != null)
                    {
                        filterCreatorId = creatorParam.Value.GetLong();
                    }
                }
            }

            QueryRelativeExpression = (query) =>
            {
                var q = query;
                // 1. 权限过滤：超级管理员或 Level >= 30 可以查看全科数据，否则只能看自己（创建的或待审核的）
                if (user.IsSuperAdmin || level >= 30)
                {
                    q = q.Where(x => x.OrganizationId == user.OrganizationId && x.DepartmentId == user.DepartmentId);
                    if (filterCreatorId > 0)
                    {
                        q = q.Where(x => x.CreatorId == filterCreatorId);
                    }
                }
                else
                {
                    q = q.Where(x => x.CreatorId == user.UserId || x.AuditUserId == user.UserId);
                }

                // 2. 默认时间过滤：如果没有传时间，默认查询未来 7 天内的数据
                if (!hasTimeFilter)
                {
                    var start = DateTime.Now.Date;
                    var end = start.AddDays(7);
                    q = q.Where(x => x.StartDate >= start && x.EndDate < end);
                }

                return q;
            };

            return base.GetPageData(options);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuting = (leave, obj) =>
            {
                // 获取当前用户信息
                var user = UserContext.Current;
                leave.OrganizationId = user.OrganizationId;
                leave.DepartmentId = user.DepartmentId;
                leave.CreatorId = user.UserId;
                leave.Creator = user.UserTrueName;
                leave.CreationTime = DateTime.Now;

                // 1. 校验开始时间
                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (leave.StartDate < monthStart)
                {
                    return new WebResponseContent().Error("开始时间不能早于本月1号");
                }

                // 2. 校验开始时间与结束时间逻辑
                if (leave.StartDate >= leave.EndDate)
                {
                    return new WebResponseContent().Error("开始时间必须早于结束时间");
                }

                // 3. 校验结束时间（最远只能请一个月内的假）
                if (leave.EndDate > DateTime.Now.AddMonths(1))
                {
                    return new WebResponseContent().Error("结束时间距离现在不能超过一个月");
                }

                // 4. 重叠校验
                bool isOverlap = _repository.Exists(x =>
                    x.CreatorId == user.UserId &&
                    x.IsDeleted != true &&
                    x.StartDate < leave.EndDate &&
                    x.EndDate > leave.StartDate);

                if (isOverlap)
                {
                    return new WebResponseContent().Error("该时间段内已存在请假记录，请勿重复提交");
                }

                return new WebResponseContent(true);
            };

            return base.Add(saveModel);
        }

        public override WebResponseContent Update(SaveModel saveModel)
        {
            UpdateOnExecuting = (leave, obj, x1, x2) =>
            {
                var user = UserContext.Current;
                leave.LastModifierId = user.UserId;
                leave.LastModifier = user.UserTrueName;
                leave.LastModificationTime = DateTime.Now;

                // 1. 校验开始时间
                var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                if (leave.StartDate < monthStart)
                {
                    return new WebResponseContent().Error("开始时间不能早于本月1号");
                }

                // 2. 校验起止顺序
                if (leave.StartDate >= leave.EndDate)
                {
                    return new WebResponseContent().Error("开始时间必须早于结束时间");
                }

                // 3. 校验最长跨度（一个月内）
                if (leave.EndDate > DateTime.Now.AddMonths(1))
                {
                    return new WebResponseContent().Error("结束时间距离现在不能超过一个月");
                }

                // 4. 重叠校验 (排除当前编辑的记录ID)
                bool isOverlap = _repository.Exists(x =>
                    x.Id != leave.Id &&
                    x.CreatorId == user.UserId &&
                    x.IsDeleted != true &&
                    x.StartDate < leave.EndDate &&
                    x.EndDate > leave.StartDate);

                if (isOverlap)
                {
                    return new WebResponseContent().Error("该时间段内已存在请假记录，请勿重复提交");
                }

                return new WebResponseContent(true);
            };

            return base.Update(saveModel);
        }
    }
}
