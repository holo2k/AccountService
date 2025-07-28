using AccountService.Infrastructure.Repository.Abstractions;
using MediatR;

namespace AccountService.Features.Account.DeleteAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Guid>
{
    private readonly IAccountRepository _repository;

    public DeleteAccountCommandHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.AccountId);

        return request.AccountId;
    }
}