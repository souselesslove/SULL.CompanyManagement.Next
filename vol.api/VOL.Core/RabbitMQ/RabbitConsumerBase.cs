using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VOL.Core.RabbitMQ;

public abstract class RabbitConsumerBase : BackgroundService
{
    protected readonly IRabbitMqPersistentConnection _connection;
    protected readonly ILogger _logger;
    protected readonly string QueueName;

    protected RabbitConsumerBase(IRabbitMqPersistentConnection connection, ILogger logger, string queueName)
    {
        _connection = connection;
        _logger = logger;
        QueueName = queueName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("消费者开始启动，队列: {Queue}", QueueName);
        var channel = await _connection.CreateChannelAsync(stoppingToken);

        //await channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        //await channel.ExchangeDeclareAsync(
        //    exchange: Define.ExchangeName,
        //    type: ExchangeType.Fanout,  // 或 "topic", "fanout"
        //    durable: true,
        //    autoDelete: false,
        //    cancellationToken: stoppingToken);
        //// 绑定队列到 Exchange（routingKey = bindingKey）
        //await channel.QueueBindAsync(
        //    queue: QueueName,
        //    exchange: Define.ExchangeName,
        //    routingKey: QueueName,
        //    cancellationToken: stoppingToken);

        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                //_logger.LogInformation("收到消息！DeliveryTag: {Tag}", ea.DeliveryTag);

                // body 现在推荐使用 Span 或 ToArray()
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                await HandleMessageAsync(json, stoppingToken);

                await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理消息失败，消息将重新入队或进入死信队列");
                await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        var consumerTag = await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("消费者注册成功，ConsumerTag: {Tag}", consumerTag);
        // 保持运行，直到应用停止
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    protected abstract Task HandleMessageAsync(string messageJson, CancellationToken cancellationToken);
}