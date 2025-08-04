using AccountService.CurrencyService.Abstractions;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class TransferBetweenAccountsCommandHandler : IRequestHandler<TransferBetweenAccountsCommand, MbResult<Guid>>
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

    public async Task<MbResult<Guid>> Handle(TransferBetweenAccountsCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.PayloadModel;

        if (!_currencyService.IsSupported(payload.Currency))
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "CurrencyNotSupported",
                Message = $"Валюта '{payload.Currency}' не поддерживается"
            });

        var fromAccount = await _accountRepository.GetByIdAsync(payload.FromAccountId);
        if (fromAccount is null)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт отправителя с ID {payload.FromAccountId} не найден"
            });

        var toAccount = await _accountRepository.GetByIdAsync(payload.ToAccountId);
        if (toAccount is null)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт получателя с ID {payload.ToAccountId} не найден"
            });

        if (fromAccount.Currency != payload.Currency || toAccount.Currency != payload.Currency)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "CurrencyMismatch",
                Message = "Валюта счёта не совпадает с валютой транзакции"
            });

        if (fromAccount.Balance < payload.Amount)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "InsufficientFunds",
                Message = $"Недостаточно средств: баланс {fromAccount.Balance}, требуется {payload.Amount}"
            });

        fromAccount.Balance -= payload.Amount;
        var updateFromResult = await _accountRepository.UpdateAsync(fromAccount);
        if (!updateFromResult.IsSuccess)
            return MbResult<Guid>.Fail(updateFromResult.Error!);

        toAccount.Balance += payload.Amount;
        var updateToResult = await _accountRepository.UpdateAsync(toAccount);
        if (!updateToResult.IsSuccess)
            return MbResult<Guid>.Fail(updateToResult.Error!);

        var debitTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = fromAccount.Id,
            CounterPartyAccountId = toAccount.Id,
            Amount = payload.Amount,
            Currency = payload.Currency,
            Type = TransactionType.Debit,
            Description = payload.Description,
            Date = DateTime.UtcNow
        };

        var creditTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = toAccount.Id,
            CounterPartyAccountId = fromAccount.Id,
            Amount = payload.Amount,
            Currency = payload.Currency,
            Type = TransactionType.Credit,
            Description = payload.Description,
            Date = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(debitTransaction);
        await _transactionRepository.AddAsync(creditTransaction);

        return MbResult<Guid>.Success(debitTransaction.Id);
    }
}