using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.UpdateAccount;

public record UpdateAccountCommand(AccountDto Account) : IRequest<MbResult<Guid>>;