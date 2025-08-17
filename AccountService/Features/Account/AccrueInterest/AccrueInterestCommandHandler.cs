using AccountService.Features.Outbox.Service;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.AccrueInterest;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AccrueInterestCommandHandler : IRequestHandler<AccrueInterestCommand, MbResult<Unit>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AppDbContext _dbContext;
    private readonly IOutboxService _outboxService;

    public AccrueInterestCommandHandler(IAccountRepository accountRepository, AppDbContext dbContext,
        IOutboxService outboxService)
    {
        _accountRepository = accountRepository;
        _dbContext = dbContext;
        _outboxService = outboxService;
    }

    public async Task<MbResult<Unit>> Handle(AccrueInterestCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId);

            if (account is null)
                return MbResult<Unit>.Fail(new MbError
                {
                    Code = "NotFound",
                    Message = $"Счёт с ID '{request.AccountId}' не был найден."
                });

            var balanceBefore = account.Balance;

            var success = await _accountRepository.AccrueInterestAsync(request.AccountId);
            if (!success)
            {
                await transaction.RollbackAsync(cancellationToken);
                return MbResult<Unit>.Fail(new MbError
                {
                    Code = "AccrueFailed",
                    Message = "Не удалось добавить проценты"
                });
            }

            var entry = _dbContext.Entry(account);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract (Проверка необходима для тестов)
            if (entry is not null)
                await entry.ReloadAsync(cancellationToken);

            var balanceAfter = account.Balance;

            var amount = balanceAfter - balanceBefore;

            var interestModel = new AccrueInterestModel
            {
                AccountId = request.AccountId,
                Amount = amount,
                PeriodFrom = DateTime.UtcNow.Date.AddDays(-1),
                PeriodTo = DateTime.UtcNow.Date
            };


            await _outboxService.AddInterestAccruedEventAsync(interestModel);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MbResult<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult<Unit>.Fail(new MbError
            {
                Code = "AccrueException",
                Message = ex.Message
            });
        }
    }
}