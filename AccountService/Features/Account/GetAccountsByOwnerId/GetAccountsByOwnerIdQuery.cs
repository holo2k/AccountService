using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.GetAccountsByOwnerId;

public record GetAccountsByOwnerIdQuery(Guid OwnerId) : IRequest<MbResult<ICollection<AccountDto>>>;