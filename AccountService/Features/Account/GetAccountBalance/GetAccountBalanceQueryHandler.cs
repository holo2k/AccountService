using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.GetAccountBalance;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, MbResult<decimal>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<MbResult<decimal>> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(request.OwnerId);

        var checkingAccount = accounts.FirstOrDefault(a => a.Type == AccountType.Checking);

        if (checkingAccount is null)
            return MbResult<decimal>.Fail(new MbError
            {
                Code = "NotFound",
                Message = "У пользователя нет текущего счёта."
            });

        return MbResult<decimal>.Success(checkingAccount.Balance);
    }
}