using MediatR;

namespace AccountService.Features.Account.GetAccount;

public record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;