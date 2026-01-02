/*
 *所有关于RescueStaff类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*RescueStaffService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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

namespace SULL.HospitalManagement.Services
{
    public partial class RescueStaffService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRescueStaffRepository _repository;//访问数据库
        private readonly IRescueRepository _rescueRepository;

        [ActivatorUtilitiesConstructor]
        public RescueStaffService(
            IRescueStaffRepository dbRepository,
            IRescueRepository rescueRepository,
            IHttpContextAccessor httpContextAccessor
            )
        : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
            _rescueRepository = rescueRepository;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            if (saveModel.MainData.TryGetValue("UserId", out var userIdObj) && 
                saveModel.MainData.TryGetValue("RescueId", out var rescueIdObj))
            {
                var userId = userIdObj.ToString().GetLong();
                var rescueId = rescueIdObj.ToString().GetLong();
                
                if (_repository.Exists(x => x.UserId == userId && x.RescueId == rescueId && x.IsDeleted != true))
                {
                    return new WebResponseContent().Error("该人员已添加，请勿重复添加");
                }
            }

            AddOnExecuted = (staff, response) =>
            {
                if (staff.RescueId > 0)
                {
                    var rescue = _rescueRepository.FindFirst(x => x.Id == staff.RescueId);
                    if (rescue != null)
                    {
                        rescue.StaffCounter = (byte)((rescue.StaffCounter ?? 0) + 1);
                        rescue.LastModifierId = UserContext.Current.UserId;
                        rescue.LastModifier = UserContext.Current.UserTrueName;
                        rescue.LastModificationTime = DateTime.Now;
                        _rescueRepository.Update(rescue, true);
                    }
                }
                return new WebResponseContent(true);
            };

            return base.Add(saveModel);
        }

        public override WebResponseContent Del(object[] keys, bool delList = true)
        {
            var staffList = new List<RescueStaff>();
            if (keys != null && keys.Length > 0)
            {
                var longKeys = keys.Select(x => x.ToString().GetLong()).ToList();
                staffList = _repository.Find(x => longKeys.Contains(x.Id)).ToList();
            }

            DelOnExecuted = (k) =>
            {
                foreach (var staff in staffList)
                {
                    if (staff.RescueId > 0)
                    {
                        var rescue = _rescueRepository.FindFirst(x => x.Id == staff.RescueId);
                        if (rescue != null)
                        {
                            rescue.StaffCounter = (byte)Math.Max(0, (rescue.StaffCounter ?? 0) - 1);
                            rescue.LastModifierId = UserContext.Current.UserId;
                            rescue.LastModifier = UserContext.Current.UserTrueName;
                            rescue.LastModificationTime = DateTime.Now;
                            _rescueRepository.Update(rescue, true);
                        }
                    }
                }
                return new WebResponseContent(true);
            };

            return base.Del(keys, delList);
        }
    }
}
