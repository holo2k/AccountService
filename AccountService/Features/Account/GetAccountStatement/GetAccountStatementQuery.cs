using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.GetAccountStatement;

public record GetAccountStatementQuery
    (Guid AccountId, DateTime From, DateTime To) : IRequest<MbResult<AccountStatementDto>>;