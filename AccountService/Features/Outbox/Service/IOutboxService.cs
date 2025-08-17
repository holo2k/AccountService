using AccountService.Features.Account;
using AccountService.Features.Transaction;

namespace AccountService.Features.Outbox.Service;

public interface IOutboxService
{
    Task AddTransactionEventAsync(Transaction.Transaction transaction, Guid? correlationId = null,
        Guid? causationId = null);

    Task AddTransferCompletedEventAsync(TransactionPayload payload, Guid? correlationId = null,
        Guid? causationId = null);

    Task AddAccountOpenedEventAsync(Account.Account account, Guid? correlationId = null, Guid? causationId = null);

    Task AddInterestAccruedEventAsync(AccrueInterestModel model, Guid? correlationId = null,
        Guid? causationId = null);

    Task AddFreezeUnfreezeClientEvent(Guid clientId, string type, Guid? correlationId = null,
        Guid? causationId = null);
}