using AccountService.Features.Account;

namespace AccountService.Infrastructure.Repository.Abstractions;

public interface IAccountRepository
{
    Task<ICollection<Account>> GetByUserIdAsync(Guid ownerId);
    Task<Account> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Guid id);
}