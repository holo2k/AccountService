using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.UpdateAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, MbResult<Guid>>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public UpdateAccountCommandHandler(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MbResult<Guid>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = _mapper.Map<Account>(request.Account);

        var result = await _repository.UpdateAsync(account);

        return !result.IsSuccess ? MbResult<Guid>.Fail(result.Error!) : MbResult<Guid>.Success(account.Id);
    }
}