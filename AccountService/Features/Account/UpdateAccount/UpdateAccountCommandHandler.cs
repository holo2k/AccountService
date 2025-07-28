using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.UpdateAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public UpdateAccountCommandHandler(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = _mapper.Map<Account>(request.Account);
        await _repository.UpdateAsync(account);

        return account.Id;
    }
}