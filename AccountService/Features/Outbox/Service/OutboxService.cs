using System.Dynamic;
using AccountService.Features.Account;
using AccountService.Features.Transaction;
using AccountService.Infrastructure.Repository;
using Newtonsoft.Json;

namespace AccountService.Features.Outbox.Service;

public class OutboxService : IOutboxService
{
    private readonly AppDbContext _dbContext;

    public OutboxService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddTransferCompletedEventAsync(
        TransactionPayload payloadModel,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        var eventId = Guid.CreateVersion7();

        var payload = new
        {
            sourceAccountId = payloadModel.FromAccountId,
            destinationAccountId = payloadModel.ToAccountId,
            amount = payloadModel.Amount,
            currency = payloadModel.Currency,
            transferId = Guid.NewGuid()
        };

        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateType = "Transfer",
            AggregateId = payloadModel.ToAccountId.ToString(),
            Type = "TransferCompleted",
            Payload = JsonConvert.SerializeObject(payload),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? Guid.NewGuid(),
            CausationId = causationId ?? eventId,
            RetryCount = 0,
            ProcessedAt = null
        });
    }

    public async Task AddTransactionEventAsync(
        Transaction.Transaction transaction,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        var eventId = Guid.CreateVersion7();
        var eventType = transaction.Type == TransactionType.Credit ? "MoneyCredited" : "MoneyDebited";

        dynamic payload = new ExpandoObject();
        payload.accountId = transaction.AccountId;
        payload.amount = transaction.Amount;
        payload.currency = transaction.Currency;
        payload.operationId = transaction.Id;

        if (transaction.Type == TransactionType.Debit)
            payload.reason = transaction.Description;

        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateType = "Transaction",
            AggregateId = transaction.Id.ToString(),
            Type = eventType,
            Payload = JsonConvert.SerializeObject(payload),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? eventId,
            CausationId = causationId ?? eventId,
            RetryCount = 0
        });
    }

    public async Task AddAccountOpenedEventAsync(Account.Account account, Guid? correlationId = null,
        Guid? causationId = null)
    {
        var eventId = Guid.CreateVersion7();

        var payload = new
        {
            accountId = account.Id,
            ownerId = account.OwnerId,
            currency = account.Currency,
            type = account.Type.ToString()
        };

        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateType = "Account",
            AggregateId = account.Id.ToString(),
            Type = "AccountOpened",
            Payload = JsonConvert.SerializeObject(payload),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? eventId,
            CausationId = causationId ?? eventId,
            RetryCount = 0
        });
    }

    public async Task AddInterestAccruedEventAsync(AccrueInterestModel model, Guid? correlationId = null,
        Guid? causationId = null)
    {
        var eventId = Guid.CreateVersion7();

        var payload = new
        {
            accountId = model.AccountId,
            periodFrom = model.PeriodFrom,
            periodTo = model.PeriodTo,
            amount = model.Amount
        };

        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateType = "Interest",
            AggregateId = model.AccountId.ToString(),
            Type = "InterestAccrued",
            Payload = JsonConvert.SerializeObject(payload),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? eventId,
            CausationId = causationId ?? eventId,
            RetryCount = 0
        });
    }

    public async Task AddFreezeUnfreezeClientEvent(Guid clientId, string type, Guid? correlationId = null,
        Guid? causationId = null)
    {
        var eventId = Guid.CreateVersion7();

        var payload = new
        {
            clientId
        };

        await _dbContext.OutboxMessages.AddAsync(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            AggregateType = "User",
            AggregateId = clientId.ToString(),
            Type = type,
            Payload = JsonConvert.SerializeObject(payload),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId ?? eventId,
            CausationId = causationId ?? eventId,
            RetryCount = 0
        });
    }
}