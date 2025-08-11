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

    public AccrueInterestCommandHandler(IAccountRepository accountRepository, AppDbContext dbContext)
    {
        _accountRepository = accountRepository;
        _dbContext = dbContext;
    }

    public async Task<MbResult<Unit>> Handle(AccrueInterestCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
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