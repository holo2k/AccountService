using System.Net.Http.Json;
using AccountService.Infrastructure.Repository;
using AccountService.PipelineBehaviors;
using AccountService.Tests.Integration.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AccountService.Tests.Integration;

[Collection("SequentialIntegrationTests")]
public class TransferEmitsSingleEventTest : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Guid _testOwnerId1 = Guid.Parse("1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656");
    private readonly Guid _testOwnerId2 = Guid.Parse("43007588-4211-492f-ace0-f5b10aefe26b");

    public TransferEmitsSingleEventTest(
        IntegrationTestFixture fixture,
        ITestOutputHelper testOutputHelper)
    {
        _fixture = fixture;
        _testOutputHelper = testOutputHelper;
        _client = fixture.Client;
    }

    [Fact]
    public async Task FiftyTransfers_ShouldEmitFiftyEvents()
    {
        var accountId1 = await CreateAccountAsync(100_000m, _testOwnerId1);
        var accountId2 = await CreateAccountAsync(100_000m, _testOwnerId2);

        for (var i = 0; i < 50; i++) await TransferAsync(accountId1, accountId2, 100);

        await WaitForCondition(
            async () =>
            {
                using var scope = _fixture.Factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return await db.OutboxMessages
                    .CountAsync(e => e.Type == "TransferCompleted" && e.ProcessedAt != null) >= 50;
            },
            30000,
            "Processing TransferCompleted events"
        );

        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var createdEvents = await db.OutboxMessages
                .Where(e => e.Type == "TransferCompleted")
                .ToListAsync();

            var processedEvents = await db.OutboxMessages
                .Where(e => e.Type == "TransferCompleted" && e.ProcessedAt != null)
                .ToListAsync();

            _testOutputHelper.WriteLine($"Created events: {createdEvents.Count}");
            _testOutputHelper.WriteLine($"Processed events: {processedEvents.Count}");

            Assert.Equal(50, createdEvents.Count);
            Assert.Equal(50, processedEvents.Count);

            var eventIds = createdEvents.Select(e => e.EventId).Distinct().ToList();
            Assert.Equal(50, eventIds.Count);

            var sampleEvent = createdEvents.First();
            Assert.NotNull(sampleEvent.Payload);
            Assert.Contains("\"sourceAccountId\":", sampleEvent.Payload);
            Assert.Contains("\"destinationAccountId\":", sampleEvent.Payload);
        }
    }

    private async Task<Guid> CreateAccountAsync(decimal initialBalance, Guid ownerId)
    {
        var response = await _client.PostAsJsonAsync("/accounts", new
        {
            Account = new
            {
                OwnerId = ownerId,
                Type = "Checking",
                Currency = "RUB",
                Balance = initialBalance
            }
        });

        var mbResult = await response.Content.ReadFromJsonAsync<MbResult<Guid>>();
        return mbResult!.Result;
    }

    private async Task TransferAsync(Guid from, Guid to, decimal amount)
    {
        var requestBody = new
        {
            PayloadModel = new
            {
                FromAccountId = from,
                ToAccountId = to,
                Amount = amount,
                Currency = "RUB",
                Description = "Перевод"
            }
        };

        var response = await _client.PostAsJsonAsync("/transactions/transfer", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _testOutputHelper.WriteLine($"Transfer failed: {response.StatusCode}, {content}");
        }
    }

    private static async Task WaitForCondition(
        Func<Task<bool>> condition,
        int timeoutMs = 30000,
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
}