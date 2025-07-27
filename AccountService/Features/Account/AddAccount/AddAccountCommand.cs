using MediatR;

namespace AccountService.Features.Account.AddAccount
{
    public record AddAccountCommand(AccountDto Account) : IRequest<AccountDto>;
}
