using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SULL.HospitalManagement.IServices.Dtos;
using VOL.Core.RabbitMQ;

namespace SULL.HospitalManagement.Services.Consumers
{
    /// <summary>
    /// 消息通知消费者 - 处理异步推送任务
    /// 从 RabbitMQ 队列中获取待发送的通知，并调用 UniPushService 进行推送。
    /// 核心目标：提高服务响应速度，实现通知发送的异步解耦与削峰填谷。
    /// </summary>
    public class NotificationConsumer : RabbitConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public NotificationConsumer(IRabbitMqPersistentConnection connection, 
            ILogger<NotificationConsumer> logger, 
            IServiceProvider serviceProvider) 
            : base(connection, logger, Define.NotificationQueue)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 消息处理逻辑
        /// </summary>
        /// <param name="messageJson">JSON 格式的消息内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        protected override async Task HandleMessageAsync(string messageJson, CancellationToken cancellationToken)
        {
            try
            {
                // 1. 反序列化消息 DTO
                var message = JsonSerializer.Deserialize<NotificationMessage>(messageJson);
                
                if (message != null && message.ClientIds != null && message.ClientIds.Any())
                {
                    // 2. 创建 Scoped 容器以获取 Scoped 服务 (如 UniPushService)
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var uniPushService = scope.ServiceProvider.GetRequiredService<IUniPushService>();
                        
                        // 3. 执行推送操作
                        await uniPushService.SendAsync(message.ClientIds, message.Title, message.Content, message.Payload);
                        
                        _logger.LogInformation($"成功处理通知消息：{message.Title}，接收人数：{message.ClientIds.Count}");
                    }
                }
            }
            catch (Exception e)
            {
                // 4. 异常处理：记录日志并抛出，由基类处理 Nack 或重试
                _logger.LogError(e, "处理通知消息失败");
                
                // 延迟 5 秒后抛出，避免死循环导致 CPU 飙升
                await Task.Delay(5000, cancellationToken);
                throw;
            }
        }
    }
}
