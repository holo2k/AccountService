using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Transaction.AddTransaction;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, MbResult<Guid>>
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

    public async Task<MbResult<Guid>> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        var transactionDto = request.Transaction;

        var account = await _accountRepository.GetByIdAsync(transactionDto.AccountId)
                      ?? throw new AccountNotFoundException(transactionDto.AccountId);

        if (transactionDto.Type == TransactionType.Debit && account.Balance < transactionDto.Amount)
            throw new InsufficientFundsException(account.Id, account.Balance, transactionDto.Amount);

        account.Balance += transactionDto.Type == TransactionType.Credit
            ? transactionDto.Amount
            : -transactionDto.Amount;

        await _accountRepository.UpdateAsync(account);

        var transaction = _mapper.Map<Transaction>(transactionDto);
        transaction.Id = Guid.CreateVersion7();
        transaction.Date = DateTime.UtcNow;

        await _transactionRepository.AddAsync(transaction);

        if (transactionDto.CounterPartyAccountId is null)
            return MbResult<Guid>.Success(transaction.Id);

        var counterPartyAccount = await _accountRepository.GetByIdAsync(transactionDto.CounterPartyAccountId.Value)
                                  ?? throw new AccountNotFoundException(transactionDto.CounterPartyAccountId.Value);

        if (counterPartyAccount.Currency != transactionDto.Currency)
            throw new CurrencyMismatchException();

        counterPartyAccount.Balance += transactionDto.Type == TransactionType.Credit
            ? -transactionDto.Amount
            : transactionDto.Amount;

        await _accountRepository.UpdateAsync(counterPartyAccount);

        var mirroredTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = counterPartyAccount.Id,
            CounterPartyAccountId = account.Id,
            Amount = transactionDto.Amount,
            Currency = transactionDto.Currency,
            Type = transactionDto.Type == TransactionType.Credit ? TransactionType.Debit : TransactionType.Credit,
            Description = transactionDto.Description,
            Date = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(mirroredTransaction);

        return MbResult<Guid>.Success(transaction.Id);
    }
}