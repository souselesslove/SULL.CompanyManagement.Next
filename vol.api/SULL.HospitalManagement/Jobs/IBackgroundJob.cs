using System;
using System.Threading;
using System.Threading.Tasks;

namespace SULL.HospitalManagement.Jobs
{
    public interface IBackgroundJob
    {
        // 动态计算下一次运行时间
        DateTime GetNextRunTime(DateTime now);
        Task ExecuteAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken);
    }
}
