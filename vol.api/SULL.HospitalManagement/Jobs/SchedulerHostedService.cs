using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection; // 确保引用此命名空间以使用 CreateScope
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SULL.HospitalManagement.Jobs
{
    public class SchedulerHostedService : IHostedService, IDisposable
    {
        private readonly IEnumerable<IBackgroundJob> _jobs;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SchedulerHostedService> _logger;
        private List<Timer> _timers;

        public SchedulerHostedService(IEnumerable<IBackgroundJob> jobs, IServiceProvider serviceProvider, ILogger<SchedulerHostedService> logger)
        {
            _jobs = jobs;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _timers = new List<Timer>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var job in _jobs)
            {
                var jobName = job.GetType().Name;
                var now = DateTime.Now;
                var nextRunTime = job.GetNextRunTime(now);
                var initialDelay = nextRunTime - now;
                if (initialDelay < TimeSpan.Zero) initialDelay = TimeSpan.Zero;

                _logger.LogInformation($"[Job Registered] {jobName} | Next Run: {nextRunTime:yyyy-MM-dd HH:mm:ss}");

                Timer timer = null;
                // 使用 Timer 实现非循环调度
                // 初始化时不立即启动 (Timeout.Infinite)，确保 timer 变量赋值完成后再启动，避免由于回调执行过快导致的 timer 为 null 的空引用问题
                timer = new Timer(async state =>
                {
                    // 暂停计时器，防止重入 (虽然Period是Infinite，但这是一个好习惯)
                    timer?.Change(Timeout.Infinite, 0);
                    try
                    {
                        _logger.LogInformation($"[Job Executing] {jobName} | Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            await job.ExecuteAsync(scope.ServiceProvider, cancellationToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Job Error] {jobName}: {ex.Message}");
                    }
                    finally
                    {
                        _logger.LogInformation($"[Job Finished] {jobName} at {DateTime.Now}");
                        // 任务执行完毕后，重启计时器
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var next = job.GetNextRunTime(DateTime.Now);
                            var delay = next - DateTime.Now;
                            if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;
                            _logger.LogInformation($"[Job Scheduled] {jobName} Next Run: {next}");
                            timer?.Change(delay, Timeout.InfiniteTimeSpan);
                        }
                    }
                }, null, Timeout.Infinite, Timeout.Infinite); 

                _timers.Add(timer);
                
                // 启动计时器
                timer.Change(initialDelay, Timeout.InfiniteTimeSpan);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var timer in _timers)
            {
                timer?.Change(Timeout.Infinite, 0);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var timer in _timers)
            {
                timer?.Dispose();
            }
        }
    }
}
