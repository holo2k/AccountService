using MediatR;

namespace AccountService.Features.Account.GetAccount
{
    public record GetAccountsQuery(Guid UserId) : IRequest<ICollection<AccountDto>>;
}
