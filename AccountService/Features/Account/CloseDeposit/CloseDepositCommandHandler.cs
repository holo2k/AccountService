using AccountService.Infrastructure.Helpers;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.CloseDeposit;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class CloseDepositCommandHandler : IRequestHandler<CloseDepositCommand, MbResult<ClosedDepositDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AppDbContext _dbContext;
    private readonly ISqlExecutor _sqlExecutor;

    public CloseDepositCommandHandler(IAccountRepository accountRepository, AppDbContext dbContext,
        ISqlExecutor sqlExecutor)
    {
        _accountRepository = accountRepository;
        _dbContext = dbContext;
        _sqlExecutor = sqlExecutor;
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
            var rowsAffected = await _sqlExecutor.ExecuteScalarIntAsync(
                "SELECT accrue_interest(@p0)", request.AccountId);

            if (rowsAffected <= 0)
                throw new Exception("Не удалось начислить проценты");

            var entry = _dbContext.Entry(account);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract (Проверка необходима для тестов)
            if (entry is not null)
                await entry.ReloadAsync(cancellationToken);

            if (account.CloseDate != null)
                throw new Exception("Вклад уже закрыт");

            account.CloseDate = DateTime.UtcNow;

            var updateResult = await _accountRepository.UpdateAsync(account);
            if (!updateResult.IsSuccess)
                throw new Exception(updateResult.Error!.Message);

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