using System.Net;
using System.Net.Http.Json;
using AccountService.PipelineBehaviors;
using AccountService.Tests.Integration.Common;
using Xunit.Abstractions;

namespace AccountService.Tests.Integration;

[Collection("SequentialIntegrationTests")]
public class ParallelTransferTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly Guid _testOwnerId1 = Guid.Parse("1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656");
    private readonly Guid _testOwnerId2 = Guid.Parse("43007588-4211-492f-ace0-f5b10aefe26b");

    public ParallelTransferTests(IntegrationTestFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = fixture.Client;
    }

    [Fact]
    public async Task TotalBalance_ShouldBeTheSame_AfterParallelTransfers()
    {
        var accountA = await CreateAccountAsync(10_000m, _testOwnerId1);
        var accountB = await CreateAccountAsync(10_000m, _testOwnerId2);

        var initialTotal = await GetTotalBalanceAsync();

        var tasks = Enumerable.Range(0, 50).Select(i =>
        {
            var from = i % 2 == 0 ? accountA : accountB;
            var to = i % 2 == 0 ? accountB : accountA;
            return TransferAsync(from, to, 100);
        }).ToList();

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r);
        var conflictCount = results.Count(r => !r);

        var finalTotal = await GetTotalBalanceAsync();

        _testOutputHelper.WriteLine($"Transfers succeeded: {successCount}, conflicts (409): {conflictCount}");
        _testOutputHelper.WriteLine($"Initial balance: {initialTotal}, final balance: {finalTotal}");

        Assert.Contains(true, results);

        Assert.Equal(initialTotal, finalTotal);
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
        if (mbResult is not { IsSuccess: true })
            throw new Exception("Failed to create account or invalid response.");

        return mbResult.Result;
    }

    private async Task<decimal> GetTotalBalanceAsync()
    {
        var mbResultA = await _client.GetFromJsonAsync<MbResult<decimal>>($"/accounts/{_testOwnerId1}/balance");
        var mbResultB = await _client.GetFromJsonAsync<MbResult<decimal>>($"/accounts/{_testOwnerId2}/balance");

        if (mbResultA is not { IsSuccess: true })
            throw new Exception($"Failed to get balance for user {_testOwnerId1}");

        if (mbResultB is not { IsSuccess: true })
            throw new Exception($"Failed to get balance for user {_testOwnerId2}");

        return mbResultA.Result + mbResultB.Result;
    }


    private async Task<bool> TransferAsync(Guid from, Guid to, decimal amount)
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

        if (response.IsSuccessStatusCode)
        {
            var mbResultA = await _client.GetFromJsonAsync<MbResult<decimal>>($"/accounts/{_testOwnerId1}/balance");
            var mbResultB = await _client.GetFromJsonAsync<MbResult<decimal>>($"/accounts/{_testOwnerId2}/balance");
            _testOutputHelper.WriteLine(
                $"Balance after transfer owner 1: {mbResultA!.Result}, owner 2: {mbResultB!.Result}");

            return true;
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
            return false;

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }

        return false;
    }
}