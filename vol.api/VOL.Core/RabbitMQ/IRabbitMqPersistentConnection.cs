using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

public interface IRabbitMqPersistentConnection : IDisposable
{
    bool IsConnected { get; }
    Task EnsureConnectedAsync(CancellationToken cancellationToken = default);
    Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default);
}

public class DefaultRabbitMqPersistentConnection : IRabbitMqPersistentConnection
{
    private readonly IConnectionFactory _factory;
    private readonly ILogger<DefaultRabbitMqPersistentConnection> _logger;
    private IConnection _connection;
    private bool _disposed;
    private readonly object _lock = new();

    public DefaultRabbitMqPersistentConnection(IConnectionFactory factory, ILogger<DefaultRabbitMqPersistentConnection> logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _logger = logger;
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected) return;

        lock (_lock)
        {
            if (IsConnected) return;
        }

        try
        {
            _connection = await _factory.CreateConnectionAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("RabbitMQ 连接建立成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ 连接失败");
            throw;
        }

        _connection.ConnectionShutdownAsync += (sender, args) =>
        {
            OnConnectionShutdown();
            return Task.CompletedTask;
        };
        _connection.CallbackExceptionAsync += (sender, args) =>
        {
            OnCallbackException();
            return Task.CompletedTask;
        };
        _connection.ConnectionBlockedAsync += (sender, args) =>
        {
            OnConnectionBlocked();
            return Task.CompletedTask;
        };
    }

    private void OnConnectionShutdown()
    {
        if (_disposed) return;
        _logger.LogWarning("RabbitMQ 连接关闭，尝试重连...");
        _ = EnsureConnectedAsync(); // 异步重连
    }

    private void OnCallbackException() => OnConnectionShutdown();
    private void OnConnectionBlocked() => OnConnectionShutdown();

    public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken);
        return await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection?.Dispose();
    }
}