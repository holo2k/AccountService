using System.Data;
using System.Text;
using AccountService.Features.Outbox;
using AccountService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccountService.Features.Account.Consumers;

/// <summary>
///     Фоновый сервис, который подписывается на очередь RabbitMQ <c>account.audit</c>
///     и обрабатывает входящие события аудита.
///     Класс обеспечивает проверку структуры сообщений, версию <c>v1</c>,
///     защиту от повторной обработки, сохранение обработанных сообщений
///     в таблицу <c>InboxConsumed</c> и запись ошибок в <c>InboxDeadLetters</c>.
/// </summary>
public class AuditConsumer : BackgroundService
{
    private const string Queue = "account.audit";
    private readonly ILogger<AuditConsumer> _logger;
    private readonly IServiceProvider _sp;
    private IChannel? _channel;
    private IConnection? _connection;

    public AuditConsumer(IServiceProvider sp, ILogger<AuditConsumer> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "rabbit-mq", ConsumerDispatchConcurrency = 1 };
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(Queue, true, false, false, cancellationToken: stoppingToken);
        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;
        await _channel.BasicConsumeAsync(Queue, false, consumer, stoppingToken);

        _logger.LogInformation("AuditConsumer started and listening on {Queue}", Queue);
    }

    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var txt = Encoding.UTF8.GetString(body);

        var messageId = Guid.Empty;
        try
        {
            var jObject = JObject.Parse(txt);

            var requiredFields = new[] { "eventId", "occurredAt", "meta" };
            foreach (var field in requiredFields)
            {
                if (jObject[field] != null) continue;
                await SaveDeadLetter(jObject, $"Missing required field: {field}");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            var meta = jObject["meta"];
            if (meta?["version"] == null || meta["version"]!.Value<string>() != "v1")
            {
                await SaveDeadLetter(jObject, "Unsupported or missing meta.version");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            Guid.TryParse(jObject["eventId"]!.Value<string>(), out messageId);

            const string handlerName = nameof(AuditConsumer);

            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var existing = await db.InboxConsumed
                .Where(i => i.MessageId == messageId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return; // уже обработано
            }

            // сохраняем факт обработки
            db.InboxConsumed.Add(new InboxConsumed
            {
                MessageId = messageId,
                Handler = handlerName,
                ProcessedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            await _channel!.BasicAckAsync(ea.DeliveryTag, false);

            _logger.LogInformation("Processed audit message {MessageId}", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit message id={MessageId}", messageId);
            try
            {
                await SaveDeadLetter(JObject.Parse(txt), ex.Message);
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch
            {
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            }
        }
    }

    private async Task SaveDeadLetter(JObject payload, string error)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var id = Guid.TryParse(payload["eventId"]?.Value<string>(), out var mid) ? mid : Guid.NewGuid();
        db.InboxDeadLetters.Add(new InboxDeadLetter
        {
            MessageId = id,
            ReceivedAt = DateTime.UtcNow,
            Handler = nameof(AuditConsumer),
            Payload = payload.ToString(Formatting.None),
            Error = error
        });
        await db.SaveChangesAsync();
        _logger.LogWarning("Saved dead letter id={Id} error={Error}", id, error);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _channel?.CloseAsync(cancellationToken)!;
        await _connection?.CloseAsync(cancellationToken)!;
    }
}