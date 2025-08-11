using AccountService.Features.Account.AccrueInterest;
using AccountService.Infrastructure.Repository.Abstractions;
using MediatR;

namespace AccountService.Jobs;

public class InterestAccrualJob
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;

    public InterestAccrualJob(IAccountRepository accountRepository, IMediator mediator)
    {
        _accountRepository = accountRepository;
        _mediator = mediator;
    }

    public async Task RunAsync()
    {
        var accounts = await _accountRepository.GetActiveDepositAccountsAsync();

        foreach (var account in accounts) await _mediator.Send(new AccrueInterestCommand(account.Id));
    }
}