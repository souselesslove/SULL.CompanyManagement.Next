/*
 * dashboard service
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
using SULL.HospitalManagement.IServices;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using VOL.Core.CacheManager;

namespace SULL.HospitalManagement.Services
{
    public class DashboardService : IDashboardService, IDependency
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IOrganizationDepartmentRepository _departmentRepository;
        private readonly ICacheService _cacheService;

        [ActivatorUtilitiesConstructor]
        public DashboardService(
            IPositionRepository positionRepository,
            IOrganizationDepartmentRepository departmentRepository,
            ICacheService cacheService)
        {
            _positionRepository = positionRepository;
            _departmentRepository = departmentRepository;
            _cacheService = cacheService;
        }

        public object GetDashboardData(PageDataOptions options)
        {
            long orgId = 0;
            if (options != null && !string.IsNullOrEmpty(options.Wheres))
            {
                var wheres = JsonConvert.DeserializeObject<List<SearchParameters>>(options.Wheres);
                var orgParam = wheres?.FirstOrDefault(x => x.Name == "OrganizationId");
                if (orgParam != null && long.TryParse(orgParam.Value?.ToString(), out long oid))
                {
                    orgId = oid;
                }
            }

            if (orgId <= 0)
            {
                return new object();
            }

            // Use caching to improve performance
            string cacheKey = $"Dashboard_Data_{orgId}";
            var cachedData = _cacheService.Get<object>(cacheKey);
            //if (cachedData != null)
            //{
            //    return cachedData;
            //}

            var db = _positionRepository.DbContext;

            // 1. Fetch all Rescue Cars (Category = 10)
            // Join with Department to get Dept Name
            var query = from p in db.Set<Position>()
                        where p.Category == 10 // Rescue Car
                        select p;

            if (orgId > 0)
            {
                query = query.Where(x => x.OrganizationId == orgId);
            }

            var cars = (from p in query
                        join d in db.Set<OrganizationDepartment>() on p.DepartmentId equals d.Id into pd
                        from dept in pd.DefaultIfEmpty()
                        select new
                        {
                            p.Id,
                            p.ItemId,
                            p.DisplayName, // Car Name or ID e.g. "01"
                            p.Status,
                            p.Remark,
                            DepartmentName = dept != null ? dept.Name : "未知科室",
                            p.DepartmentId
                        }).ToList();

            // 2. KPIs
            int total = cars.Count;
            int ready = cars.Count(x => (x.Status ?? 1) == 1); // 1 = Normal (Green)
            int inUse = cars.Count(x => x.Status == 2); // 2 = In Use (Blue)
            int alerts = cars.Count(x => (x.Status ?? 1) >= 3); // 3, 4 = Alert/Abnormal (Yellow/Red)
            
            // Calculate Item Stats
            // 1. Get IDs of all Rescue Cars
            var carIds = cars.Select(c => c.Id).ToList();
            
            // 2. Find Items located IN these Rescue Cars (Position.WarehouseId = Car.Id)
            var boundItemIds = db.Set<Position>()
                .Where(p => p.OrganizationId == orgId && p.ItemId != null)
                .Select(p => p.ItemId)
                .Distinct()
                .ToList();

            // 3. Calculate Stats based on these Items
            var items = db.Set<Item>()
                .Where(x => boundItemIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Price, x.AvailableStock })
                .ToList();
                
            int itemCounter = items.Count;
            long totalValue = items.Sum(x => (long)(x.Price ?? 0) * (x.AvailableStock ?? 0));
            
            // Adjust assumption: If status is not 1 or 2, count as alert.

            // 3. Departments Status Array
            // Group cars by Department. 
            // If any car in dept is Red/Yellow/Blue, set status accordingly.
            // Priority: Red > Yellow > Blue > Green
            var deptStatusList = cars.GroupBy(x => x.DepartmentName)
                .Select(g => new
                {
                    name = g.Key,
                    status = GetDeptStatus(g.Select(c => c.Status ?? 1))
                })
                .ToList();

            // 4. Alert Cars (Drum Cars)
            var alertCars = cars.Where(x => (x.Status ?? 1) >= 2)
                .Select(x => new
                {
                    id = x.DisplayName ?? x.Id.ToString(),
                    dept = x.DepartmentName,
                    type = GetStatusColor(x.Status ?? 1),
                    msg = x.Remark ?? GetStatusMsg(x.Status ?? 1),
                    loc = x.Remark // Placeholder for location
                }).ToList();

            // 5. Mock Charts (Can be refined with real data later)
            // 5. Abnormal Classification Charts
            // TODO: Implement real logic for calculating these counts
            int nearExpiry = alerts; 
            int expired = 0;
            int stockout = 0;
            int handover = 0;
            int sealed_count = 0; // 'sealed' is a keyword in C# (sealed class), using sealed_count
            int maintain = 0;

            var charts = new List<object>
            {
                new { name = "近效期", val = nearExpiry, pct = total > 0 ? (int)((double)nearExpiry/total*100) : 0, color = "var(--yellow)" },
                new { name = "过期", val = expired, pct = total > 0 ? (int)((double)expired/total*100) : 0, color = "var(--red)" },
                new { name = "库存", val = stockout, pct = total > 0 ? (int)((double)stockout/total*100) : 0, color = "var(--blue)" },
                new { name = "交接班", val = handover, pct = total > 0 ? (int)((double)handover/total*100) : 0, color = "var(--green)" },
                new { name = "封存", val = sealed_count, pct = total > 0 ? (int)((double)sealed_count/total*100) : 0, color = "var(--red)" },
                 new { name = "维保", val = maintain, pct = total > 0 ? (int)((double)maintain/total*100) : 0, color = "var(--blue)" }
            };

            // 6. Mock Ranks
            var ranks = new List<object>
            {
                new { name = "重症医学科", score = 100 },
                new { name = "心血管内科", score = 99.8 },
                new { name = "神经外科", score = 99.5 },
                new { name = "儿科", score = 98.2 },
                new { name = "骨科", score = 97.0 }
            };

            // 7. Logs from Cache
            string logCacheKey = $"Dashboard_Logs_{orgId}";
            var logs = _cacheService.Get<List<object>>(logCacheKey) ?? new List<object>();

            // Mock Logs (Fall back if empty for demo, remove later)
            if (logs.Count == 0)
            {
               logs = new List<object>
               {
                   new { time = DateTime.Now.AddMinutes(-5).ToString("HH:mm:ss"), type = "正常", msg = "系统自动巡检完成" } 
               };
            }

            var result = new
            {
                kpis = new { total, ready, inUse, alerts, itemCounter, totalValue },
                charts,
                ranks,
                depts = deptStatusList,
                drumCars = alertCars,
                logs
            };

            // Cache data for 60 seconds
            _cacheService.Add(cacheKey, JsonConvert.SerializeObject(result), 60);

            return result;
        }

        public void AddLog(long orgId, string type, string msg)
        {
            string logCacheKey = $"Dashboard_Logs_{orgId}";
            var logs = _cacheService.Get<List<object>>(logCacheKey) ?? new List<object>();
            
            logs.Insert(0, new { time = DateTime.Now.ToString("HH:mm:ss"), type, msg });
            
            if (logs.Count > 50)
            {
                logs = logs.Take(50).ToList();
            }
            
            _cacheService.Add(logCacheKey, JsonConvert.SerializeObject(logs));
        }

        private string GetDeptStatus(IEnumerable<byte> statuses)
        {
            if (statuses.Any(s => s >= 4)) return "red";
            if (statuses.Any(s => s == 3)) return "yellow";
            if (statuses.Any(s => s == 2)) return "blue";
            return "green";
        }

        private string GetStatusColor(byte status)
        {
            if (status >= 4) return "red";
            if (status == 3) return "yellow";
            if (status == 2) return "blue";
            return "green";
        }

        private string GetStatusMsg(byte status)
        {
             if (status >= 4) return "异常";
             if (status == 3) return "警示";
             if (status == 2) return "使用中";
             return "正常";
        }
    }
}
