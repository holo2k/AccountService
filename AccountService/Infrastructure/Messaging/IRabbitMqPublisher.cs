namespace AccountService.Infrastructure.Messaging;

public interface IRabbitMqPublisher
{
    bool IsConnected();
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task PublishAsync(string routingKey, string payload, IDictionary<string, object>? headers = null,
        CancellationToken cancellationToken = default);

    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}