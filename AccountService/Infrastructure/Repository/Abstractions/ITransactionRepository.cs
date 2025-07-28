using AccountService.Features.Transaction;

namespace AccountService.Infrastructure.Repository.Abstractions;

public interface ITransactionRepository
{
    Task<IEnumerable<Transaction>> GetByAccountIdAsync(Guid accountId);
    Task AddAsync(Transaction transaction);
}