using AccountService.Exceptions;
using AccountService.Features.Account;
using AccountService.Infrastructure.Repository.Abstractions;

namespace AccountService.Infrastructure.Repository.Implementations;

public class AccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new();

    public Task<ICollection<Account>> GetByUserIdAsync(Guid ownerId)
    {
        return Task.FromResult<ICollection<Account>>(_accounts.Where(x => x.OwnerId == ownerId).ToList());
    }

    public Task<Account> GetByIdAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id) ?? throw new AccountNotFoundException(id);
        return Task.FromResult(account);
    }

    public Task AddAsync(Account account)
    {
        _accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Account account)
    {
        var index = _accounts.FindIndex(a => a.Id == account.Id);

        if (index < 0)
            throw new AccountNotFoundException(account.Id);

        _accounts[index] = account;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account is null)
            throw new AccountNotFoundException(id);

        _accounts.Remove(account);

        return Task.CompletedTask;
    }
}