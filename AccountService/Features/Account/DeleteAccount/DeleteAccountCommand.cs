using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.DeleteAccount;

public record DeleteAccountCommand(Guid AccountId) : IRequest<MbResult<Guid>>;