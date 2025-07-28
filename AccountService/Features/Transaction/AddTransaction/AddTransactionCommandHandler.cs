using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Transaction.AddTransaction;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IMapper _mapper;
    private readonly ITransactionRepository _transactionRepository;

    public AddTransactionCommandHandler(ITransactionRepository transactionRepository,
        IAccountRepository accountRepository, IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<Guid> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Transaction.AccountId)
                      ?? throw new AccountNotFoundException(request.Transaction.AccountId);

        if (request.Transaction.Type == TransactionType.Debit && account.Balance < request.Transaction.Amount)
            throw new InsufficientFundsException(account.Id, account.Balance, request.Transaction.Amount);

        account.Balance += request.Transaction.Type == TransactionType.Credit
            ? request.Transaction.Amount
            : -request.Transaction.Amount;

        await _accountRepository.UpdateAsync(account);

        var transaction = _mapper.Map<Transaction>(request.Transaction);
        transaction.Id = Guid.CreateVersion7();
        transaction.Date = DateTime.Now;

        await _transactionRepository.AddAsync(transaction);

        return transaction.Id;
    }
}