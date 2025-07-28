using AccountService.Features.Transaction;
using AccountService.Infrastructure.Repository.Abstractions;

namespace AccountService.Infrastructure.Repository.Implementations;

public class TransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();

    public Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId)
    {
        return Task.FromResult(_transactions.Where(t => t.AccountId == accountId));
    }

    public Task AddAsync(Transaction transaction)
    {
        _transactions.Add(transaction);
        return Task.CompletedTask;
    }
}