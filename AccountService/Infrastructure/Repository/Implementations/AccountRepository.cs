using AccountService.Features.Account;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure.Repository.Implementations;

public class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _dbContext;

    public AccountRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ICollection<Account>> GetByUserIdAsync(Guid ownerId)
    {
        return await _dbContext.Accounts
            .Where(a => a.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Account account)
    {
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<MbResult<Unit>> UpdateAsync(Account account)
    {
        var exists = await _dbContext.Accounts
            .AnyAsync(a => a.Id == account.Id);

        if (!exists)
            return MbResult<Unit>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {account.Id} не найден"
            });

        _dbContext.Accounts.Update(account);
        await _dbContext.SaveChangesAsync();

        return MbResult<Unit>.Success(Unit.Value);
    }

    public async Task<MbResult<Unit>> DeleteAsync(Guid id)
    {
        var account = await _dbContext.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account is null)
            return MbResult<Unit>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {id} не найден"
            });

        _dbContext.Accounts.Remove(account);
        await _dbContext.SaveChangesAsync();

        return MbResult<Unit>.Success(Unit.Value);
    }
}