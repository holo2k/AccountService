using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.GetAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountQueryHandler(IAccountRepository accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account is null)
            throw new AccountNotFoundException(request.AccountId);

        return _mapper.Map<AccountDto>(account);
    }
}