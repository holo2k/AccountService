using MediatR;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(TransactionPayload PayloadModel) : IRequest<Guid>;