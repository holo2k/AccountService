using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.UpdateAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, MbResult<Guid>>
{
    private readonly IAccountRepository _repository;

    public UpdateAccountCommandHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<MbResult<Guid>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await _repository.GetByIdAsync(request.Account.Id);

        if (existingAccount == null)
            return MbResult<Guid>.Fail(new MbError { Code = "NotFound", Message = "Account not found" });

        existingAccount.Type = request.Account.Type;
        existingAccount.Currency = request.Account.Currency;
        existingAccount.PercentageRate = request.Account.PercentageRate;

        var result = await _repository.UpdateAsync(existingAccount);

        return !result.IsSuccess ? MbResult<Guid>.Fail(result.Error!) : MbResult<Guid>.Success(existingAccount.Id);
    }
}