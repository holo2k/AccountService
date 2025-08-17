using System.Text;
using RabbitMQ.Client;

namespace AccountService.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly string _exchangeName;
    private readonly string _host;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly string _pass;
    private readonly int _port;
    private readonly string _user;
    private IChannel _channel = null!;
    private IConnection? _connection;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger,
        string host = "rabbit-mq", int port = 5672, string user = "guest", string pass = "guest",
        string exchangeName = "account.events")
    {
        _logger = logger;
        _host = host;
        _port = port;
        _user = user;
        _pass = pass;
        _exchangeName = exchangeName;
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            _channel.CloseAsync();
            _channel.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing channel");
        }

        try
        {
            _connection?.CloseAsync();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing connection");
        }

        return ValueTask.CompletedTask;
    }

    public Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_connection?.IsOpen == true && _channel.IsOpen);
    }

    public async Task PublishAsync(string routingKey, string payload, IDictionary<string, object>? headers = null,
        CancellationToken cancellationToken = default)
    {
        if (_channel == null || _connection == null)
            throw new InvalidOperationException("RabbitMqPublisher not initialized");

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object>()!
        };

        if (headers != null)
            foreach (var kv in headers)
                props.Headers[kv.Key] = kv.Value;

        if (!props.Headers.ContainsKey("X-Correlation-Id"))
            props.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();

        if (!props.Headers.ContainsKey("X-Causation-Id"))
            props.Headers["X-Causation-Id"] = Guid.NewGuid().ToString();

        var body = Encoding.UTF8.GetBytes(payload);


        await _channel.BasicPublishAsync(_exchangeName,
            routingKey,
            false,
            props,
            body, cancellationToken);
        _logger.LogInformation(
            "Published message to {Exchange} / {RoutingKey} Size={Size} Correlation={Correlation}",
            _exchangeName, routingKey, body.Length, props.Headers["X-Correlation-Id"]);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _host,
            Port = _port,
            UserName = _user,
            Password = _pass
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic, true, false,
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ initialized. Exchange: {Exchange}", _exchangeName);
    }

    public bool IsConnected()
    {
        return _connection?.IsOpen == true && _channel.IsOpen;
    }
}