using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.AccrueInterest;

public record AccrueInterestCommand(Guid AccountId) : IRequest<MbResult<Unit>>;