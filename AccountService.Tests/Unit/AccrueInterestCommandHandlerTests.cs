using AccountService.Features.Account.AccrueInterest;
using Moq;

namespace AccountService.Tests.Unit;

public class AccrueInterestCommandHandlerTests : UnitTestBase
{
    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_AccrueInterestSucceeds()
    {
        var accountId = Guid.NewGuid();

        var handler = new AccrueInterestCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object);

        var result = await handler.Handle(new AccrueInterestCommand(accountId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediatR.Unit.Value, result.Result);
        AccountRepositoryMock.Verify(r => r.AccrueInterestAsync(accountId), Times.Once);
        DbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        DbContextTransactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFail_When_AccrueInterestFails()
    {
        var accountId = Guid.NewGuid();

        AccountRepositoryMock
            .Setup(r => r.AccrueInterestAsync(accountId))
            .ReturnsAsync(false);

        var handler = new AccrueInterestCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object);

        var result = await handler.Handle(new AccrueInterestCommand(accountId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccrueFailed", result.Error?.Code);
        DbContextTransactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        DbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnFail_When_ExceptionThrown()
    {
        var accountId = Guid.NewGuid();

        AccountRepositoryMock
            .Setup(r => r.AccrueInterestAsync(accountId))
            .ThrowsAsync(new InvalidOperationException("Some DB error"));

        var handler = new AccrueInterestCommandHandler(AccountRepositoryMock.Object, DbContextMock.Object);

        var result = await handler.Handle(new AccrueInterestCommand(accountId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("AccrueException", result.Error?.Code);
        Assert.Contains("Some DB error", result.Error?.Message);
        DbContextTransactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        DbContextTransactionMock.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}