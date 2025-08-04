using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.CheckAccountOwnership;

public record CheckAccountOwnershipQuery(Guid OwnerId, Guid AccountId) : IRequest<MbResult<bool>>;