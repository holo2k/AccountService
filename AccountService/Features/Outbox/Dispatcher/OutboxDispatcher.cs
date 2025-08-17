using System.Diagnostics;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AccountService.Features.Outbox.Dispatcher;

/// <summary>
///     Сервис фоновой публикации событий из таблицы Outbox в RabbitMQ.
///     Реализует pattern "Outbox" для гарантированной доставки сообщений:
///     - Читает из базы данных <see cref="OutboxMessage" /> записи, у которых <c>ProcessedAt</c> не заполнено.
///     - Формирует сообщение в формате JSON с обязательным <c>meta</c> блоком.
///     - Определяет routing key на основе типа события (<see cref="MapEventTypeToRoutingKey" />).
///     - Публикует сообщение в RabbitMQ с заголовками корреляции.
///     - При успешной отправке помечает запись как обработанную (<c>ProcessedAt</c>, <c>PublishedLatencyMs</c>).
///     - В случае ошибки увеличивает счетчик <c>RetryCount</c> и при превышении <c>MaxRetries</c> переносит в dead-letter.
///     Особенности:
///     - Работает в бесконечном цикле с паузой 1 секунда между итерациями.
///     - Обрабатывает до 20 событий за итерацию, в порядке возрастания времени <c>OccurredAt</c>.
///     - При parse некорректного JSON payload оборачивает его в объект с полем <c>raw</c>.
///     - Логи успешных отправлений, предупреждений и ошибок.
///     - При отмене токена корректно завершает работу.
///     Использование:
///     - Регистрируется в DI как <see cref="BackgroundService" />.
///     - Требует <see cref="AppDbContext" /> для доступа к БД и <see cref="IRabbitMqPublisher" /> для публикации
///     сообщений.
/// </summary>
public class OutboxDispatcher : BackgroundService
{
    private const int MaxRetries = 50;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly IServiceProvider _sp;

    public OutboxDispatcher(IServiceProvider sp, ILogger<OutboxDispatcher> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

                var events = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.OccurredAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var evt in events)
                    try
                    {
                        var routingKey = MapEventTypeToRoutingKey(evt.Type);
                        var eventId = evt.EventId != Guid.Empty ? evt.EventId : Guid.NewGuid();
                        var correlationId = evt.CorrelationId != Guid.Empty ? evt.CorrelationId : Guid.NewGuid();
                        var causationId = evt.CausationId != Guid.Empty ? evt.CausationId : Guid.NewGuid();

                        var meta = new JObject
                        {
                            ["version"] = "v1",
                            ["source"] = "account-service",
                            ["correlationId"] = correlationId.ToString(),
                            ["causationId"] = causationId.ToString()
                        };

                        JObject payloadObj;
                        try
                        {
                            payloadObj = string.IsNullOrWhiteSpace(evt.Payload)
                                ? new JObject()
                                : JObject.Parse(evt.Payload);
                        }
                        catch (Exception)
                        {
                            payloadObj = new JObject { ["raw"] = evt.Payload };
                        }

                        var envelope = new JObject
                        {
                            ["eventId"] = eventId.ToString(),
                            ["type"] = evt.Type,
                            ["occurredAt"] = evt.OccurredAt.ToString("o"),
                            ["meta"] = meta,
                            ["payload"] = new JObject(payloadObj)
                        };

                        var envelopeJson = envelope.ToString(Formatting.None);

                        var headers = new Dictionary<string, object>
                        {
                            ["X-Correlation-Id"] = correlationId.ToString(),
                            ["X-Causation-Id"] = causationId.ToString()
                        };

                        var sw = Stopwatch.StartNew();
                        await rabbit.PublishAsync(routingKey, envelopeJson, headers, stoppingToken);
                        sw.Stop();

                        evt.ProcessedAt = DateTime.UtcNow;
                        evt.PublishedLatencyMs = (int)sw.ElapsedMilliseconds;
                        evt.RetryCount = 0;
                        db.OutboxMessages.Update(evt);

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Event published: {EventId}, Type: {Type}, Correlation: {CorrelationId}, Latency: {Latency}ms",
                            eventId,
                            evt.Type,
                            correlationId,
                            sw.ElapsedMilliseconds
                        );
                    }
                    catch (Exception exEv)
                    {
                        _logger.LogWarning(exEv, "Failed to publish outbox event id={OutboxId} type={Type}", evt.Id,
                            evt.Type);

                        evt.RetryCount++;
                        evt.LastError = exEv.Message;
                        db.OutboxMessages.Update(evt);

                        if (evt.RetryCount >= MaxRetries)
                            await SaveDeadLetter(evt, exEv.Message);

                        if (stoppingToken.IsCancellationRequested)
                            break;

                        await DelayWithBackOffAsync(evt.RetryCount, stoppingToken);
                    }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OutboxDispatcher shutting down (cancellation requested).");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxDispatcher runloop failed");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            try
            {
                await Task.Delay(1000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OutboxDispatcher stopping (delay interrupted).");
                break;
            }
        }

        _logger.LogInformation("OutboxDispatcher stopped.");
    }

    private static string MapEventTypeToRoutingKey(string eventType)
    {
        return eventType switch
        {
            "AccountOpened" => "account.opened",
            "MoneyCredited" => "money.credited",
            "MoneyDebited" => "money.debited",
            "TransferCompleted" => "money.transfer.completed",
            "InterestAccrued" => "money.interest.accrued",
            "ClientBlocked" => "client.blocked",
            "ClientUnblocked" => "client.unblocked",
            _ => throw new InvalidOperationException($"Unknown event type mapping for '{eventType}'")
        };
    }

    private async Task SaveDeadLetter(OutboxMessage evt, string error)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var id = evt.EventId != Guid.Empty ? evt.EventId : Guid.NewGuid();

        db.InboxDeadLetters.Add(new InboxDeadLetter
        {
            MessageId = id,
            ReceivedAt = DateTime.UtcNow,
            Handler = nameof(OutboxDispatcher),
            Payload = evt.Payload,
            Error = error
        });

        await db.SaveChangesAsync();

        _logger.LogWarning("Saved dead letter id={Id} error={Error}", id, error);
    }

    private static async Task DelayWithBackOffAsync(int retryCount, CancellationToken ct)
    {
        var baseDelay = TimeSpan.FromMilliseconds(500);
        var maxDelay = TimeSpan.FromSeconds(30);

        // Экспоненциальная задержка: 500ms * 2^retryCount
        var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, retryCount));

        if (delay > maxDelay)
            delay = maxDelay;

        var jitterPercent = Random.Shared.NextDouble() * 0.4 - 0.2;
        var jitterDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitterPercent));

        await Task.Delay(jitterDelay, ct);
    }
}