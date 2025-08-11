using System.Data;
using AccountService.CurrencyService.Abstractions;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class TransferBetweenAccountsCommandHandler
    : IRequestHandler<TransferBetweenAccountsCommand, MbResult<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrencyService _currencyService;
    private readonly AppDbContext _dbContext;
    private readonly ITransactionRepository _transactionRepository;

    public TransferBetweenAccountsCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        ICurrencyService currencyService,
        AppDbContext dbContext)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _currencyService = currencyService;
        _dbContext = dbContext;
    }

    public async Task<MbResult<Guid>> Handle(
        TransferBetweenAccountsCommand request,
        CancellationToken cancellationToken)
    {
        var payload = request.PayloadModel;

        if (!_currencyService.IsSupported(payload.Currency))
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "CurrencyNotSupported",
                Message = $"Валюта '{payload.Currency}' не поддерживается"
            });

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var fromAccount = await _accountRepository.GetByIdAsync(payload.FromAccountId);
            if (fromAccount is null)
                return FailAndRollback(transaction, "NotFound",
                    $"Счёт отправителя с ID {payload.FromAccountId} не найден");

            var toAccount = await _accountRepository.GetByIdAsync(payload.ToAccountId);
            if (toAccount is null)
                return FailAndRollback(transaction, "NotFound",
                    $"Счёт получателя с ID {payload.ToAccountId} не найден");

            if (fromAccount.Currency != payload.Currency || toAccount.Currency != payload.Currency)
                return FailAndRollback(transaction, "CurrencyMismatch",
                    "Валюта счёта не совпадает с валютой транзакции");

            if (fromAccount.Balance < payload.Amount)
                return FailAndRollback(transaction, "InsufficientFunds",
                    $"Недостаточно средств: баланс {fromAccount.Balance}, требуется {payload.Amount}");

            var fromInitialBalance = fromAccount.Balance;
            var toInitialBalance = toAccount.Balance;

            fromAccount.Balance -= payload.Amount;
            toAccount.Balance += payload.Amount;

            var updateFromResult = await _accountRepository.UpdateAsync(fromAccount);
            if (!updateFromResult.IsSuccess)
                return FailAndRollback(transaction, updateFromResult.Error!.Code, updateFromResult.Error.Message);

            var updateToResult = await _accountRepository.UpdateAsync(toAccount);
            if (!updateToResult.IsSuccess)
                return FailAndRollback(transaction, updateToResult.Error!.Code, updateToResult.Error.Message);

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

            await _dbContext.SaveChangesAsync(cancellationToken);

            var fromExpected = fromInitialBalance - payload.Amount;
            var toExpected = toInitialBalance + payload.Amount;

            if (fromAccount.Balance != fromExpected || toAccount.Balance != toExpected)
                throw new InvalidOperationException(
                    "Несоответствие баланса после перевода. " +
                    $"Ожидалось: From={fromExpected}, To={toExpected}. " +
                    $"Фактически: From={fromAccount.Balance}, To={toAccount.Balance}");

            await transaction.CommitAsync(cancellationToken);

            return MbResult<Guid>.Success(debitTransaction.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "TransferError",
                Message = ex.Message
            });
        }
    }

    private static MbResult<Guid> FailAndRollback(IDbContextTransaction transaction, string code, string message)
    {
        transaction.Rollback();
        return MbResult<Guid>.Fail(new MbError
        {
            Code = code,
            Message = message
        });
    }
}