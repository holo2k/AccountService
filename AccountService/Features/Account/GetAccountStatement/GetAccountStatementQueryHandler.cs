using AccountService.Exceptions;
using AccountService.Features.Transaction;
using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.GetAccountStatement;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountStatementQueryHandler : IRequestHandler<GetAccountStatementQuery, AccountStatementDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly ITransactionRepository _transactionRepository;

    public GetAccountStatementQueryHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<AccountStatementDto> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId)
                      ?? throw new AccountNotFoundException(request.AccountId);

        var transactions = await _transactionRepository.GetByAccountIdAsync(request.AccountId);

        var filteredTransactions = transactions
            .Where(t => t.Date >= request.From && t.Date <= request.To)
            .OrderBy(t => t.Date)
            .ToList();

        var transactionDtoList = _mapper.Map<List<TransactionDto>>(filteredTransactions);

        return new AccountStatementDto
        {
            AccountId = account.Id,
            OwnerId = account.OwnerId,
            Currency = account.Currency,
            Type = account.Type,
            Balance = account.Balance,
            Transactions = transactionDtoList
        };
    }
}