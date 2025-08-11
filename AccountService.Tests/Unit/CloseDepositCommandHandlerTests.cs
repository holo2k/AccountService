using AccountService.Features.Account;
using AccountService.Features.Account.CloseDeposit;
using AccountService.PipelineBehaviors;
using Moq;
using Xunit.Abstractions;

namespace AccountService.Tests.Unit;

public class CloseDepositCommandHandlerTests : UnitTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CloseDepositCommandHandlerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Should_Fail_When_Account_Not_Found()
    {
        AccountRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Account)null!);

        var handler = new CloseDepositCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object,
            SqlExecutorMock.Object);

        var result = await handler.Handle(new CloseDepositCommand(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error!.Code);
    }

    [Fact]
    public async Task Should_Fail_When_Account_Is_Not_Deposit()
    {
        var account = new Account { Id = Guid.NewGuid(), Type = AccountType.Checking };
        AccountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        var handler = new CloseDepositCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object,
            SqlExecutorMock.Object);

        var result = await handler.Handle(new CloseDepositCommand(account.Id), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidAccountType", result.Error!.Code);
    }

    [Fact]
    public async Task Should_Close_Deposit_When_Valid()
    {
        var account = new Account { Id = Guid.NewGuid(), Type = AccountType.Deposit };

        AccountRepositoryMock.Setup(r => r.GetByIdAsync(account.Id))
            .ReturnsAsync(account);

        AccountRepositoryMock.Setup(r => r.UpdateAsync(account))
            .ReturnsAsync(MbResult<MediatR.Unit>.Success(MediatR.Unit.Value));

        SqlExecutorMock.Setup(s => s.ExecuteScalarIntAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync(1);

        var handler = new CloseDepositCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object,
            SqlExecutorMock.Object);

        var result = await handler.Handle(new CloseDepositCommand(account.Id), default);

        if (!result.IsSuccess)
            _testOutputHelper.WriteLine($"Error Code: {result.Error?.Code}, Message: {result.Error?.Message}");

        Assert.True(result.IsSuccess);
    }
}