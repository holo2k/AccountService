using AccountService.Features.Account;
using AccountService.Infrastructure.Helpers;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    protected readonly Mock<ISqlExecutor> SqlExecutorMock;

    protected UnitTestBase()
    {
        var options = new DbContextOptions<AppDbContext>();
        DbContextMock = new Mock<AppDbContext>(options);

        DbContextTransactionMock = new Mock<IDbContextTransaction>();
        DatabaseMock = new Mock<DatabaseFacade>(DbContextMock.Object);
        SqlExecutorMock = new Mock<ISqlExecutor>();

        DbContextMock.SetupGet(c => c.Database).Returns(DatabaseMock.Object);

        DatabaseMock
            .Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DbContextTransactionMock.Object);

        DbContextMock
            .Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        DbContextMock
            .Setup(c => c.Entry(It.IsAny<Account>()))
            .Returns((Account _) =>
            {
                var entryMock = new Mock<EntityEntry<Account>>();
                entryMock.Setup(e => e.ReloadAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
                return entryMock.Object;
            });

        DbContextMock.Setup(c => c.Entry(It.IsAny<Account>()))
            .Returns((Account _) => null!);

        SqlExecutorMock
            .Setup(x => x.ExecuteScalarIntAsync(It.IsAny<string>(), It.IsAny<object[]>()))
            .ReturnsAsync(0);

        AccountRepositoryMock
            .Setup(r => r.AccrueInterestAsync(It.IsAny<Guid>()))
            .ReturnsAsync(true);
    }
}