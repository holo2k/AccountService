using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.GetAccountsByOwnerId;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountsByOwnerIdQueryHandler : IRequestHandler<GetAccountsByOwnerIdQuery, ICollection<AccountDto>>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public GetAccountsByOwnerIdQueryHandler(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ICollection<AccountDto>> Handle(GetAccountsByOwnerIdQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await _repository.GetByUserIdAsync(request.OwnerId);

        return _mapper.Map<ICollection<AccountDto>>(accounts);
    }
}