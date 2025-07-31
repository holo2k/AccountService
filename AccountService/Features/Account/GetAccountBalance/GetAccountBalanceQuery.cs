using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.GetAccountBalance;

public record GetAccountBalanceQuery(Guid OwnerId) : IRequest<MbResult<decimal>>;