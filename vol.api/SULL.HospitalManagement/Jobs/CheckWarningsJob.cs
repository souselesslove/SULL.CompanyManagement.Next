using SULL.HospitalManagement.IServices;
using SULL.HospitalManagement.IRepositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace SULL.HospitalManagement.Jobs
{
    public class CheckWarningsJob : IBackgroundJob
    {
        // 每天早上6点执行
        public DateTime GetNextRunTime(DateTime now)
        {
             // 目标时间：当天的 06:00:00
             var target = now.Date.AddHours(6);
             
             // 如果当前时间已经过了今天的 06:00:00，则计划到明天的 06:00:00
             if (now > target)
             {
                 target = target.AddDays(1);
             }
             return target;
        }

        public async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            var itemRepo = serviceProvider.GetService<IItemRepository>();
            var itemService = serviceProvider.GetService<IItemService>();

            if (itemRepo == null || itemService == null) return;

            // 获取所有未删除物品的ID
            // 分批处理或者直接遍历，考虑到性能，如果数据量大应该分页。
            // 暂时假设数据量在可控范围内，直接获取ID列表
            var itemIds = await itemRepo.FindAsIQueryable(x => x.IsDeleted != true)
                                        .Select(x => x.Id)
                                        .ToListAsync(stoppingToken);

            foreach (var id in itemIds)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    await itemService.CheckItemWarningsAsync(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"CheckItemWarnings failed for Item {id}: {ex.Message}");
                }
            }
        }
    }
}
