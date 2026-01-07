/*
 *所有关于ItemStatusHistory类的业务代码应在此处编写
*可使用repository.调用常用方法，获取EF/Dapper等信息
*如果需要事务请使用repository.DbContextBeginTransaction
*也可使用DBServerProvider.手动获取数据库相关信息
*用户信息、权限、角色等使用UserContext.Current操作
*ItemStatusHistoryService对增、删、改查、导入、导出、审核业务代码扩展参照ServiceFunFilter
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
using Yitter.IdGenerator;

namespace SULL.HospitalManagement.Services
{
    public partial class ItemStatusHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IItemStatusHistoryRepository _repository; //访问数据库

        [ActivatorUtilitiesConstructor]
        public ItemStatusHistoryService(
            IItemStatusHistoryRepository dbRepository,
            IHttpContextAccessor httpContextAccessor
        )
            : base(dbRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _repository = dbRepository;
        }

        public override WebResponseContent Add(SaveModel saveModel)
        {
            AddOnExecuting = (statusHistory, context) =>
            {
                var db = _repository.DbContext;
                string operatorName = UserContext.Current.UserTrueName;
                string statusName = GetStatusName(statusHistory.Status, statusHistory.Category ?? 10);

                if (statusHistory.Category == 10) // 设备 (Item)
                {
                    var item = db.Set<Item>().FirstOrDefault(x => x.Id == statusHistory.ItemId);
                    if (item != null)
                    {
                        item.Status = statusHistory.Status;
                        item.StatusId = statusHistory.Id;
                        item.Remark = statusHistory.Remark;
                        db.Entry(item).State = EntityState.Modified;
                        statusHistory.DisplayName = $"{operatorName}将[{item.Name}]标记为[{statusName}]状态";
                    }
                }
                else if (statusHistory.Category == 20) // 抢救车 (Position)
                {
                    var position = db.Set<Position>().FirstOrDefault(x => x.Id == statusHistory.ItemId);
                    if (position != null)
                    {
                        position.Status = statusHistory.Status;
                        position.StatusId = statusHistory.Id;
                        position.Remark = statusHistory.Remark;
                        db.Entry(position).State = EntityState.Modified;
                        statusHistory.DisplayName = $"{operatorName}将[{position.DisplayName}]标记为[{statusName}]状态";
                    }
                }

                if (statusHistory.Status == 10 || statusHistory.Status == 20 || statusHistory.Status == 30)
                {
                    db.Set<WarningLogs>().Add(new WarningLogs
                    {
                        Id = YitIdHelper.NextId(),
                        Category = 60, // 状态异常
                        Description = statusHistory.DisplayName,
                        OrganizationId = statusHistory.OrganizationId,
                        DepartmentId = statusHistory.DepartmentId,
                        RelatedId = statusHistory.ItemId,
                        CreationTime = DateTime.Now,
                        IsFinished = false
                    });
                }

                return new WebResponseContent(true);
            };
            return base.Add(saveModel);
        }

        private string GetStatusName(byte status, byte category)
        {
            if (category == 10)
            {
                return status switch
                {
                    0 => "正常",
                    10 => "异常",
                    11 => "确认中",
                    20 => "脏污",
                    21 => "清洁中",
                    30 => "报修",
                    31 => "维修中",
                    _ => status.ToString()
                };
            }
            if (category == 20)
            {
                return status switch
                {
                    0 => "正常",
                    10 => "异常",
                    11 => "确认中",
                    20 => "脏污",
                    21 => "清洁中",
                    30 => "报修",
                    31 => "维修中",
                    _ => status.ToString()
                };
            }
            return status.ToString();
        }
    }
}

