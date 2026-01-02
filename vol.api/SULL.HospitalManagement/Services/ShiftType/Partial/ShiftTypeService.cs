/*
 *所有关于ShiftType类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ShiftTypeService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using Yitter.IdGenerator;

namespace SULL.HospitalManagement.Services
{
    public partial class ShiftTypeService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShiftTypeRepository _repository; //访问数据库

        [ActivatorUtilitiesConstructor]
        public ShiftTypeService(
            IShiftTypeRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
        )
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            //多租户会用到这init代码，其他情况可以不用
            //base.Init(dbRepository);
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuting = (shift, obj) =>
            {
                // Force use of current user's organization and department
                shift.OrganizationId = UserContext.Current.OrganizationId;
                shift.DepartmentId = UserContext.Current.DepartmentId;

                // 1. Check for Duplicate Name within the same Organization and Department
                if (_repository.Exists(x =>
                        x.Title == shift.Title && x.OrganizationId == shift.OrganizationId &&
                        x.DepartmentId == shift.DepartmentId && x.IsDeleted != true))
                {
                    return new WebResponseContent().Error($"排班名称[{shift.Title}]已存在");
                }

                // 2. Check for Time Overlap
                // Normalize times to minutes from 00:00 for comparison
                // If EndTime <= StartTime, it implies the shift crosses midnight (add 24 hours = 1440 minutes)
                var newStart = shift.StartTime.Hour * 60 + shift.StartTime.Minute;
                var newEnd = shift.EndTime.Hour * 60 + shift.EndTime.Minute;
                if (newEnd <= newStart) newEnd += 1440;

                // Fetch existing shifts to check against
                var existingShifts = _repository.Find(x =>
                    x.OrganizationId == shift.OrganizationId && x.DepartmentId == shift.DepartmentId &&
                    x.Category == shift.Category && x.IsDeleted != true);

                foreach (var exist in existingShifts)
                {
                    var existStart = exist.StartTime.Hour * 60 + exist.StartTime.Minute;
                    var existEnd = exist.EndTime.Hour * 60 + exist.EndTime.Minute;
                    if (existEnd <= existStart) existEnd += 1440;

                    // Check for intersection: Max(StartA, StartB) < Min(EndA, EndB)
                    if (Math.Max(newStart, existStart) < Math.Min(newEnd, existEnd))
                    {
                        return new WebResponseContent().Error(
                            $"与现有排班[{exist.Title} ({exist.StartTime:HH:mm}-{exist.EndTime:HH:mm})]时间冲突");
                    }
                }

                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        public WebResponseContent GenerateDefaultShifts(SaveModel saveModel)
        {
            if (saveModel == null || !saveModel.MainData.ContainsKey("Category"))
            {
                 return new WebResponseContent().Error("参数错误：缺少 Category");
            }
            int category = saveModel.MainData["Category"].GetInt();
            var organizationId = UserContext.Current.OrganizationId;
            var departmentId = UserContext.Current.DepartmentId;

            if (organizationId <= 0)
            {
                 return new WebResponseContent().Error("当前用户未绑定机构");
            }

            var shifts = new List<ShiftType>();
            // 10 = Example 3-shift system, others = 2-shift system
            if (category == 10)
            {
                shifts.Add(CreateShift("A班", "08:00", "16:00", category, organizationId, departmentId));
                shifts.Add(CreateShift("P班", "16:00", "00:00", category, organizationId, departmentId));
                shifts.Add(CreateShift("N班", "00:00", "08:00", category, organizationId, departmentId));
            }
            else
            {
                shifts.Add(CreateShift("早班", "08:00", "18:00", category, organizationId, departmentId));
                shifts.Add(CreateShift("晚班", "18:00", "08:00", category, organizationId, departmentId));
            }

            _repository.AddRange(shifts, true);
            return new WebResponseContent(true).OK("默认排班数据生成成功");
        }

        private ShiftType CreateShift(string title, string start, string end, int category, long? orgId, long? deptId)
        {
            var today = DateTime.Now.Date;
            var startTime = today.Add(TimeSpan.Parse(start));
            var endTime = today.Add(TimeSpan.Parse(end));

            // If end time is earlier than start time, it means it's the next day (for duration calc)
            // Note: For storage we usually keep just the time part or normalized date, 
            // but here we follow the detected DateTime pattern.
            // Adjust endTime date if needed for duration calculation
            var durationEnd = endTime <= startTime ? endTime.AddDays(1) : endTime;
            var duration = (int)(durationEnd - startTime).TotalMinutes;

            return new ShiftType
            {
                Id = YitIdHelper.NextId(),
                Title = title,
                Category = (byte)category,
                StartTime = startTime,
                EndTime = endTime,
                Duration = duration,
                OrganizationId = orgId,
                DepartmentId = deptId,
                CreatorId = UserContext.Current.UserId,
                Creator = UserContext.Current.UserTrueName,
                CreationTime = DateTime.Now,
                IsDeleted = false,
                OrderBy = 0
            };
        }
    }
}
