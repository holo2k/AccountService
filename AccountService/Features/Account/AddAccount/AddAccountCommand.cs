using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.AddAccount;

public record AddAccountCommand(AddAccountRequest Account) : IRequest<MbResult<Guid>>;