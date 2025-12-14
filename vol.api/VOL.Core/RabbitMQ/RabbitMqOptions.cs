using System.Collections.Generic;

namespace VOL.Core.RabbitMQ;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "cmf";
    public string Password { get; set; } = "cmf@FKHYT21619";
    public string VirtualHost { get; set; } = "/cmf";
    public string ClientProvidedName { get; set; } = "CMF";
    public List<ExchangeConfig> Exchanges { get; set; } = new();
    public List<QueueConfig> Queues { get; set; } = new();
    public List<BindingConfig> Bindings { get; set; } = new();
}

public class ExchangeConfig
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "direct"; // direct, topic, fanout, headers
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object>? Arguments { get; set; }
}

public class QueueConfig
{
    public string QueueName { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
    public Dictionary<string, object>? Arguments { get; set; }
}

public class BindingConfig
{
    public string QueueName { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public Dictionary<string, object>? Arguments { get; set; }
}