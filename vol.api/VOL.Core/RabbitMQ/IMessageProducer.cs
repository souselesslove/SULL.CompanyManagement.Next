using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using RabbitMQ.Client;
using System;
using VOL.Core.RabbitMQ;

public interface IMessageProducer
{
    Task PublishAsync<T>(T message, string exchange = "", string routingKey = "", CancellationToken ct = default);
}

public class RabbitMessageProducer : IMessageProducer
{
    private readonly IRabbitMqPersistentConnection _connection;
    private readonly ILogger<RabbitMessageProducer> _logger;

    public RabbitMessageProducer(IRabbitMqPersistentConnection connection, ILogger<RabbitMessageProducer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string exchange = Define.ExchangeName, string routingKey = "", CancellationToken ct = default)
    {
        var channel = await _connection.CreateChannelAsync(ct);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json).AsMemory();  // 推荐使用 ReadOnlyMemory<byte>

        var properties = new BasicProperties
        {
            Persistent = true  // 消息持久化
            // 可选添加其他属性，如 ContentType = "application/json"
        };

        // 正确调用 BasicPublishAsync（带 mandatory 参数）
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: false,  // 通常为 false，如果消息无法路由则返回给生产者
            basicProperties: properties,
            body: body,
            cancellationToken: ct);

        _logger.LogInformation("已发布消息到 {RoutingKey}", routingKey);
    }
}