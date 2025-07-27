using MediatR;

namespace AccountService.Features.Account.DeleteAccount
{
    public record DeleteAccountCommand(Guid AccountId) : IRequest<Guid>;
}
