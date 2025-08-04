using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(TransactionPayload PayloadModel) : IRequest<MbResult<Guid>>;