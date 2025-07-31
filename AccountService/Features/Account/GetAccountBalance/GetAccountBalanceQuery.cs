using MediatR;

namespace AccountService.Features.Account.GetAccountBalance;

public record GetAccountBalanceQuery(Guid OwnerId) : IRequest<decimal>;