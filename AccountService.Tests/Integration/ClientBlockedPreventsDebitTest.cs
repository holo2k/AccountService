using System.Net;
using System.Net.Http.Json;
using AccountService.Features.Account.FreezeAccount;
using AccountService.Infrastructure.Repository;
using AccountService.PipelineBehaviors;
using AccountService.Tests.Integration.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccountService.Tests.Integration;

[Collection("SequentialIntegrationTests")]
public class ClientBlockedPreventsDebitTest : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestFixture _fixture;

    public ClientBlockedPreventsDebitTest(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Client;
    }

    [Fact]
    public async Task ClientBlockedPreventsDebit()
    {
        var ownerId = Guid.Parse("4650ec28-5afc-4bb2-8f47-90550012646e");
        var accountId = await CreateAccountAsync(1000m, ownerId);

        await BlockClientAsync(ownerId);

        await WaitForCondition(
            async () =>
            {
                using var scope = _fixture.Factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var account = await db.Accounts.FindAsync(accountId);
                return account?.IsFrozen == true;
            },
            description: "Account freezing"
        );

        var response = await DebitAccountAsync(accountId);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        await WaitForCondition(
            async () =>
            {
                using var scope = _fixture.Factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                return !await db.OutboxMessages
                    .AnyAsync(e => e.Type == "MoneyDebited");
            },
            description: "MoneyDebited event absence"
        );
    }

    private async Task BlockClientAsync(Guid clientId)
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new FreezeAccountCommand(clientId, true));

        if (!result.IsSuccess)
            throw new Exception($"Client blocking failed: {result.Error!.Message}");
    }

    private async Task<HttpResponseMessage> DebitAccountAsync(Guid accountId)
    {
        return await _client.PostAsJsonAsync("/transactions", new
        {
            Transaction = new
            {
                AccountId = accountId, Amount = 100m, Currency = "RUB", Type = "Debit", Description = "Списание тест"
            }
        });
    }

    private async Task<Guid> CreateAccountAsync(decimal balance, Guid ownerId)
    {
        var response = await _client.PostAsJsonAsync("/accounts", new
        {
            Account = new
            {
                OwnerId = ownerId,
                Type = "Checking",
                Currency = "RUB",
                Balance = balance
            }
        });

        var result = await response.Content.ReadFromJsonAsync<MbResult<Guid>>();
        return result!.Result;
    }


    private static async Task WaitForCondition(
        Func<Task<bool>> condition,
        int timeoutMs = 60000,
        string description = "Condition")
    {
        var startTime = DateTime.UtcNow;
        var attempts = 0;

        while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(timeoutMs))
        {
            attempts++;
            if (await condition())
            {
                Console.WriteLine($"{description} met after {attempts} attempts");
                return;
            }

            Console.WriteLine($"{description} not met yet, attempt {attempts}");
            await Task.Delay(5000);
        }

        throw new TimeoutException($"{description} not met within {timeoutMs}ms");
    }
}