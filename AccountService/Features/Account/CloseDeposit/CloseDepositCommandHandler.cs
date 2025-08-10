using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Features.Account.CloseDeposit;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class CloseDepositCommandHandler : IRequestHandler<CloseDepositCommand, MbResult<ClosedDepositDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AppDbContext _dbContext;

    public CloseDepositCommandHandler(IAccountRepository accountRepository, AppDbContext dbContext)
    {
        _accountRepository = accountRepository;
        _dbContext = dbContext;
    }

    public async Task<MbResult<ClosedDepositDto>> Handle(CloseDepositCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account is null)
            return MbResult<ClosedDepositDto>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {request.AccountId} не найден"
            });

        if (account.Type != AccountType.Deposit)
            return MbResult<ClosedDepositDto>.Fail(new MbError
            {
                Code = "InvalidAccountType",
                Message = "Можно закрыть только вклад (депозитный) счёт"
            });

        if (account.CloseDate != null)
            return MbResult<ClosedDepositDto>.Fail(new MbError
            {
                Code = "AlreadyClosed",
                Message = "Вклад уже закрыт"
            });

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "CALL accrue_interest({0})",
                new object[] { request.AccountId }, cancellationToken);

            await _dbContext.Entry(account).ReloadAsync(cancellationToken);

            if (account.CloseDate != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return MbResult<ClosedDepositDto>.Fail(new MbError
                {
                    Code = "AlreadyClosed",
                    Message = "Вклад уже закрыт"
                });
            }

            account.CloseDate = DateTime.UtcNow;

            var updateResult = await _accountRepository.UpdateAsync(account);
            if (!updateResult.IsSuccess)
            {
                await transaction.RollbackAsync(cancellationToken);
                return MbResult<ClosedDepositDto>.Fail(updateResult.Error!);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var dto = new ClosedDepositDto
            {
                AccountId = account.Id,
                CloseDate = account.CloseDate.Value,
                Balance = account.Balance
            };

            return MbResult<ClosedDepositDto>.Success(dto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult<ClosedDepositDto>.Fail(new MbError
            {
                Code = "AccrueFailed",
                Message = ex.Message
            });
        }
    }
}