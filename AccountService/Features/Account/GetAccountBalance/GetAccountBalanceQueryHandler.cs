using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using MediatR;

namespace AccountService.Features.Account.GetAccountBalance;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, decimal>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountBalanceQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<decimal> Handle(GetAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(request.OwnerId);

        var checkingAccount = accounts.FirstOrDefault(a => a.Type == AccountType.Checking);

        if (checkingAccount is null)
            throw new AccountNotFoundException("У пользователя нет текущего счёта.");

        return checkingAccount.Balance;
    }
}