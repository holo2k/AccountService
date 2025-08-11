using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.CloseDeposit;

public record CloseDepositCommand(Guid AccountId) : IRequest<MbResult<ClosedDepositDto>>;