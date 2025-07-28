using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.AddAccount;

// ReSharper disable once UnusedMember.Global (Используется MediatR)
public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public AddAccountCommandHandler(
        IAccountRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var account = _mapper.Map<Account>(request.Account);
        account.Id = Guid.CreateVersion7();
        account.OpenDate = DateTime.UtcNow;

        await _repository.AddAsync(account);

        return account.Id;
    }
}