using AccountService.CurrencyService.Abstractions;
using AccountService.Exceptions;
using AccountService.Infrastructure.Repository.Abstractions;
using MediatR;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class TransferBetweenAccountsCommandHandler : IRequestHandler<TransferBetweenAccountsCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrencyService _currencyService;
    private readonly ITransactionRepository _transactionRepository;

    public TransferBetweenAccountsCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ICurrencyService currencyService)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _currencyService = currencyService;
    }

    public async Task<Guid> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        if (!_currencyService.IsSupported(request.PayloadModel.Currency))
            throw new CurrencyNotSupportedException(request.PayloadModel.Currency);

        var fromAccount = await _accountRepository.GetByIdAsync(request.PayloadModel.FromAccountId)
                          ?? throw new AccountNotFoundException(request.PayloadModel.FromAccountId);

        var toAccount = await _accountRepository.GetByIdAsync(request.PayloadModel.ToAccountId)
                        ?? throw new AccountNotFoundException(request.PayloadModel.ToAccountId);

        if (fromAccount.Currency != request.PayloadModel.Currency ||
            toAccount.Currency != request.PayloadModel.Currency)
            throw new CurrencyMismatchException();

        if (fromAccount.Balance < request.PayloadModel.Amount)
            throw new InsufficientFundsException(fromAccount.Id, fromAccount.Balance, request.PayloadModel.Amount);

        fromAccount.Balance -= request.PayloadModel.Amount;
        await _accountRepository.UpdateAsync(fromAccount);

        toAccount.Balance += request.PayloadModel.Amount;
        await _accountRepository.UpdateAsync(toAccount);

        var debitTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = fromAccount.Id,
            CounterPartyAccountId = toAccount.Id,
            Amount = request.PayloadModel.Amount,
            Currency = request.PayloadModel.Currency,
            Type = TransactionType.Debit,
            Description = request.PayloadModel.Description,
            Date = DateTime.UtcNow
        };

        var creditTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = toAccount.Id,
            CounterPartyAccountId = fromAccount.Id,
            Amount = request.PayloadModel.Amount,
            Currency = request.PayloadModel.Currency,
            Type = TransactionType.Credit,
            Description = request.PayloadModel.Description,
            Date = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(debitTransaction);
        await _transactionRepository.AddAsync(creditTransaction);

        return debitTransaction.Id;
    }
}