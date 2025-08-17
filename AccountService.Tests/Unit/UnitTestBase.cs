using AccountService.Features.Account;
using AccountService.Features.Outbox.Service;
using AccountService.Features.Transaction;
using AccountService.Infrastructure.Helpers;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;

namespace AccountService.Tests.Unit;

public abstract class UnitTestBase
{
    protected readonly Mock<IAccountRepository> AccountRepositoryMock = new();
    protected readonly Mock<DatabaseFacade> DatabaseMock;
    protected readonly Mock<AppDbContext> DbContextMock;
    protected readonly Mock<IDbContextTransaction> DbContextTransactionMock;
    protected readonly Mock<IOutboxService> OutboxServiceMock;
    protected readonly Mock<ISqlExecutor> SqlExecutorMock;

    protected UnitTestBase()
    {
        var options = new DbContextOptions<AppDbContext>();
        DbContextMock = new Mock<AppDbContext>(options);

        DbContextTransactionMock = new Mock<IDbContextTransaction>();
        DatabaseMock = new Mock<DatabaseFacade>(DbContextMock.Object);
        SqlExecutorMock = new Mock<ISqlExecutor>();
        OutboxServiceMock = new Mock<IOutboxService>();

        DbContextMock.SetupGet(c => c.Database).Returns(DatabaseMock.Object);

        DatabaseMock
            .Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DbContextTransactionMock.Object);

        DbContextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        DbContextMock.Setup(c => c.Entry(It.IsAny<Account>()))
            .Returns((Account _) => null!);

        SqlExecutorMock
            .Setup(x => x.ExecuteScalarIntAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync(0);

        AccountRepositoryMock
            .Setup(r => r.AccrueInterestAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);

        AccountRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Account
            {
                Id = new Guid(),
                Balance = 1000,
                Currency = "RUB",
                IsFrozen = false,
                OpenDate = DateTime.Now,
                Type = AccountType.Deposit,
                PercentageRate = 50
            });

        OutboxServiceMock
            .Setup(x => x.AddAccountOpenedEventAsync(It.IsAny<Account>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);

        OutboxServiceMock
            .Setup(x => x.AddTransactionEventAsync(It.IsAny<Transaction>(), It.IsAny<Guid?>(), It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);

        OutboxServiceMock
            .Setup(x => x.AddTransferCompletedEventAsync(It.IsAny<TransactionPayload>(), It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);

        OutboxServiceMock
            .Setup(x => x.AddInterestAccruedEventAsync(It.IsAny<AccrueInterestModel>(), It.IsAny<Guid?>(),
                It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);
    }
}