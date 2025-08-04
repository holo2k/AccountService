using AccountService.Features.Account;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Infrastructure.Repository.Implementations;

public class AccountRepository : IAccountRepository
{
    private readonly List<Account> _accounts = new();

    public Task<ICollection<Account>> GetByUserIdAsync(Guid ownerId)
    {
        return Task.FromResult<ICollection<Account>>(_accounts.Where(x => x.OwnerId == ownerId).ToList());
    }

    public Task<Account?> GetByIdAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(account);
    }

    public Task AddAsync(Account account)
    {
        _accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task<MbResult<Unit>> UpdateAsync(Account account)
    {
        var index = _accounts.FindIndex(a => a.Id == account.Id);

        if (index < 0)
            return Task.FromResult(MbResult<Unit>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {account.Id} не найден"
            }));

        _accounts[index] = account;
        return Task.FromResult(MbResult<Unit>.Success(Unit.Value));
    }

    public Task<MbResult<Unit>> DeleteAsync(Guid id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);

        if (account is null)
            return Task.FromResult(MbResult<Unit>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {id} не найден"
            }));

        _accounts.Remove(account);
        return Task.FromResult(MbResult<Unit>.Success(Unit.Value));
    }
}