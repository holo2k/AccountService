using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.GetAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, MbResult<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountQueryHandler(IAccountRepository accountRepository, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<MbResult<AccountDto>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account is null)
            throw new AccountNotFoundException(request.AccountId);

        var dto = _mapper.Map<AccountDto>(account);

        return MbResult<AccountDto>.Success(dto);
    }
}