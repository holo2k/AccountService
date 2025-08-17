using System.Text;
using AccountService.Features.Outbox;
using AccountService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AccountService.Features.Account.Consumers;

// ReSharper disable once IdentifierTypo (В словаре нет слова)
/// <summary>
///     Фоновый сервис для обработки событий, связанных с заморозкой аккаунта
///     из очереди RabbitMQ.
///     Назначение:
///     - Подписывается на топик <c>client.#</c> в обменнике <c>account.events</c>.
///     - Получает события о заморозке/разморозки клиентов.
///     - Обновляет состояние счетов клиента (<c>IsFrozen</c>) в зависимости от события.
///     - Фиксирует факт обработки события в таблице <c>InboxConsumed</c>.
///     - При ошибках или некорректных сообщениях сохраняет в <c>InboxDeadLetters</c>.
///     Поддерживаемые типы событий:
///     - <c>ClientBlocked</c> — заморозка всех счетов клиента.
///     - <c>ClientUnblocked</c> — разморозка всех счетов клиента.
///     Особенности:
///     - Проверяет наличие обязательных полей в сообщении (<c>eventId</c>, <c>type</c>, <c>meta</c>, <c>occurredAt</c>).
///     - Проверка версии (<c>meta.version</c> == "v1").
///     - Не обрабатывает событие повторно, если <c>InboxConsumed</c> уже содержит <c>MessageId</c>.
///     - Использует транзакции EF Core для согласованного обновления счетов и записи о потреблении.
///     - Сохраняет dead-letter при превышении ошибок или отсутствии данных.
///     - Настроен на обработку сообщений по одному за раз (<c>BasicQos</c>).
///     Подключения:
///     - <see cref="IConnection" /> и <see cref="IChannel" /> RabbitMQ для получения сообщений.
///     - <see cref="AppDbContext" /> для доступа к БД.
/// </summary>
public class AntifraudConsumer : BackgroundService
{
    private const string Exchange = "account.events";

    // ReSharper disable once StringLiteralTypo (В словаре нет слова)
    private const string Queue = "account.antifraud";
    private const string Routing = "client.#";
    private readonly ILogger<AntifraudConsumer> _logger;
    private readonly IServiceProvider _sp;
    private IChannel? _channel;
    private IConnection? _connection;

    // ReSharper disable once IdentifierTypo (В словаре нет слова)
    public AntifraudConsumer(IServiceProvider sp, ILogger<AntifraudConsumer> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit-mq";
        var port = Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672";
        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = int.Parse(port),
            ConsumerDispatchConcurrency = 1
        };
        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(Queue, true, false, false, cancellationToken: stoppingToken);
        await _channel.QueueBindAsync(Queue, Exchange, Routing, cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(0, 1, false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageAsync;
        await _channel.BasicConsumeAsync(Queue, false, consumer, stoppingToken);

        // ReSharper disable once StringLiteralTypo (В словаре нет слова)
        _logger.LogInformation("AntifraudConsumer started and bound to {Queue}", Queue);
    }


    private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var txt = Encoding.UTF8.GetString(body);

        var messageId = Guid.Empty;
        try
        {
            var jObject = JObject.Parse(txt);

            var requiredFields = new[] { "eventId", "meta", "occurredAt" };
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

            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var eventName = jObject["type"]?.Value<string>();
            var payload = jObject["payload"];

            if (string.IsNullOrEmpty(eventName))
            {
                await SaveDeadLetter(jObject, "Event type is null or empty");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }

            if (payload?["clientId"] == null ||
                !Guid.TryParse((payload["clientId"] ?? throw new InvalidOperationException()).Value<string>(),
                    out var clientId))
            {
                await SaveDeadLetter(jObject, "Missing or invalid clientId in payload");
                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }


            _logger.LogDebug(
                "Received message: {EventId}, Type: {Type}, Correlation: {CorrelationId}",
                messageId,
                eventName,
                jObject["meta"]?["correlationId"]?.Value<string>()
            );

            switch (eventName)
            {
                case "ClientBlocked":
                case "ClientUnblocked":
                    await HandleClientBlockStatusAsync(eventName, clientId, messageId, ea.DeliveryTag, db);
                    return;

                default:
                    await _channel!.BasicAckAsync(ea.DeliveryTag, false);
                    break;
            }

            _logger.LogInformation(
                "Processed {EventType} for client {ClientId}",
                eventName,
                clientId
            );
        }
        catch (Exception ex)
        {
            // ReSharper disable once StringLiteralTypo (В словаре нет слова)
            _logger.LogError(ex, "Error processing antifraud message id={MessageId}", messageId);
            try
            {
                await SaveDeadLetter(JObject.Parse(Encoding.UTF8.GetString(ea.Body.ToArray())), ex.Message);
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
            Handler = nameof(AntifraudConsumer),
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

    private async Task HandleClientBlockStatusAsync(
        string eventName,
        Guid clientId,
        Guid messageId,
        ulong deliveryTag,
        AppDbContext db)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var existing = await db.InboxConsumed
                .Where(i => i.MessageId == messageId && i.Handler == nameof(AntifraudConsumer))
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                await _channel!.BasicAckAsync(deliveryTag, false);
                return;
            }

            var accounts = await db.Accounts
                .Where(a => a.OwnerId == clientId)
                .ToListAsync();

            foreach (var a in accounts)
                a.IsFrozen = eventName == "ClientBlocked";

            db.InboxConsumed.Add(new InboxConsumed
            {
                MessageId = messageId,
                Handler = nameof(AntifraudConsumer),
                ProcessedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            await _channel!.BasicAckAsync(deliveryTag, false);

            _logger.LogInformation("{EventName} processed for client {ClientId}", eventName, clientId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}