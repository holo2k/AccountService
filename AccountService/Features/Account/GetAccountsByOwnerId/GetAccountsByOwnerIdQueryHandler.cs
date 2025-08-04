using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.GetAccountsByOwnerId;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class
    GetAccountsByOwnerIdQueryHandler : IRequestHandler<GetAccountsByOwnerIdQuery, MbResult<ICollection<AccountDto>>>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public GetAccountsByOwnerIdQueryHandler(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MbResult<ICollection<AccountDto>>> Handle(GetAccountsByOwnerIdQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetByUserIdAsync(request.OwnerId);

        var accountsDto = _mapper.Map<ICollection<AccountDto>>(accounts);

        return MbResult<ICollection<AccountDto>>.Success(accountsDto);
    }
}