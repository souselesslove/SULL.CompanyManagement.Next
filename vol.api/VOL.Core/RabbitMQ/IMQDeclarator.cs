using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static VOL.Core.RabbitMQ.IMQDeclarator;
using System.Collections.Generic;

namespace VOL.Core.RabbitMQ;

public interface IMQDeclarator
{
    Task MQDeclareAsync(CancellationToken ct = default);

    public class MQDeclarator : IMQDeclarator
    {
        private readonly IRabbitMqPersistentConnection _connection;
        private readonly IOptions<RabbitMqOptions> _options;
        private readonly ILogger<MQDeclarator> _logger;

        public MQDeclarator(IRabbitMqPersistentConnection connection,
            IOptions<RabbitMqOptions> options,
            ILogger<MQDeclarator> logger)
        {
            _connection = connection;
            _options = options;
            _logger = logger;
        }

        public async Task MQDeclareAsync(CancellationToken ct = default)
        {
            var channel = await _connection.CreateChannelAsync(ct);

            // 1. 声明 Exchanges
            foreach (var ex in _options.Value.Exchanges)
            {
                await channel.ExchangeDeclareAsync(
                    exchange: ex.Name,
                    type: ex.Type,
                    durable: ex.Durable,
                    autoDelete: ex.AutoDelete,
                    arguments: ex.Arguments,
                    cancellationToken: ct);
                _logger.LogInformation("Exchange 已声明: {Name}", ex.Name);
            }

            // 2. 声明 Queues
            foreach (var q in _options.Value.Queues)
            {
                var args = q.Arguments ?? new Dictionary<string, object>();

                // 手动修正常见数值参数，确保是 long 类型
                if (args.TryGetValue("x-message-ttl", out var ttlObj) && ttlObj is string ttlStr)
                {
                    if (long.TryParse(ttlStr, out var ttlLong))
                    {
                        args["x-message-ttl"] = ttlLong;  // 转为 long
                    }
                }
                if (args.TryGetValue("x-max-length", out var maxLenObj) && maxLenObj is string maxLenStr)
                {
                    if (long.TryParse(maxLenStr, out var maxLenLong))
                    {
                        args["x-max-length"] = maxLenLong;
                    }
                }

                await channel.QueueDeclareAsync(
                    queue: q.QueueName,
                    durable: q.Durable,
                    exclusive: q.Exclusive,
                    autoDelete: q.AutoDelete,
                    arguments: q.Arguments,
                    cancellationToken: ct);
                _logger.LogInformation("Queue 已声明: {Name}", q.QueueName);
            }

            // 3. 绑定
            foreach (var b in _options.Value.Bindings)
            {
                await channel.QueueBindAsync(
                    queue: b.QueueName,
                    exchange: b.ExchangeName,
                    routingKey: b.RoutingKey,
                    arguments: b.Arguments,
                    cancellationToken: ct);
                _logger.LogInformation("Binding 已创建: {Queue} <- {Exchange} [{RoutingKey}]", b.QueueName, b.ExchangeName, b.RoutingKey);
            }
        }
    }
}