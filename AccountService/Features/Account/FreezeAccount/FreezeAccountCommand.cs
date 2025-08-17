using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.FreezeAccount;

public record FreezeAccountCommand(
    Guid ClientId,
    bool IsFrozen
) : IRequest<MbResult<Guid>>;