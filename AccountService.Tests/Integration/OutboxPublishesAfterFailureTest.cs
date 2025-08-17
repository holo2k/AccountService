using System.Net.Http.Json;
using AccountService.Features.Outbox;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repository;
using AccountService.Tests.Integration.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Tests.Integration;

[Collection("SequentialIntegrationTests")]
public class OutboxPublishesAfterFailureTest : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public OutboxPublishesAfterFailureTest(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task OutboxPublishesAfterFailure()
    {
        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();
            Assert.True(rabbit.IsConnected());
        }

        await _fixture.StopRabbitMqAsync();

        await WaitForCondition(() =>
            {
                using var scope = _fixture.Factory.Services.CreateScope();
                var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();
                return Task.FromResult(!rabbit.IsConnected());
            },
            10000,
            "RabbitMQ connection drop detection"
        );

        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();
            Assert.False(rabbit.IsConnected(), "RabbitMQ should be disconnected after stop");
        }

        var ownerId = Guid.Parse("4650ec28-5afc-4bb2-8f47-90550012646e");

        await CreateAccountAsync(1000m, ownerId);

        OutboxMessage outboxEvent;
        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            outboxEvent = (await db.OutboxMessages
                .FirstOrDefaultAsync(e => e.Type == "AccountOpened"))!;

            Assert.NotNull(outboxEvent);
            Assert.Null(outboxEvent.ProcessedAt);
        }

        await _fixture.StartRabbitMqAsync();

        await WaitForRabbitMqConnection();

        await WaitForCondition(
            async () =>
            {
                using var scope = _fixture.Factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var updatedEvent = await db.OutboxMessages.FindAsync(outboxEvent.Id);
                return updatedEvent?.ProcessedAt != null;
            },
            30000,
            "Outbox event processing"
        );

        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var processedEvent = await db.OutboxMessages.FindAsync(outboxEvent.Id);
            Assert.NotNull(processedEvent?.ProcessedAt);
        }
    }

    private async Task WaitForRabbitMqConnection(int maxAttempts = 10)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            using var scope = _fixture.Factory.Services.CreateScope();
            var rabbit = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

            if (rabbit.IsConnected()) return;

            await Task.Delay(1000);
        }

        throw new Exception("RabbitMQ connection not restored in time");
    }

    private static async Task WaitForCondition(
        Func<Task<bool>> condition,
        int timeoutMs = 10000,
        string description = "Condition")
    {
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(timeoutMs))
        {
            if (await condition()) return;
            await Task.Delay(500);
        }

        throw new TimeoutException($"{description} not met within {timeoutMs}ms");
    }

    private async Task CreateAccountAsync(decimal balance, Guid ownerId)
    {
        await _client.PostAsJsonAsync("/accounts", new
        {
            Account = new
            {
                OwnerId = ownerId,
                Type = "Checking",
                Currency = "RUB",
                Balance = balance
            }
        });
    }
}