using AccountService.Features.Account;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Infrastructure.Repository.Abstractions;

public interface IAccountRepository
{
    Task<ICollection<Account>> GetByUserIdAsync(Guid ownerId);
    Task<ICollection<Account>> GetActiveDepositAccountsAsync();
    Task<Account?> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
    Task<MbResult<Unit>> UpdateAsync(Account account);
    Task<MbResult<Unit>> DeleteAsync(Guid id);
    Task<bool> AccrueInterestAsync(Guid accountId);
}