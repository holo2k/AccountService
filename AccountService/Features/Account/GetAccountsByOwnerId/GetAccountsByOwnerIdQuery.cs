using MediatR;

namespace AccountService.Features.Account.GetAccountsByOwnerId;

public record GetAccountsByOwnerIdQuery(Guid OwnerId) : IRequest<ICollection<AccountDto>>;