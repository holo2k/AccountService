using System.Data;
using AccountService.Infrastructure.Repository;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AccountService.Features.Transaction.AddTransaction;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, MbResult<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ITransactionRepository _transactionRepository;

    public AddTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IMapper mapper,
        AppDbContext dbContext)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<MbResult<Guid>> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Transaction;

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        try
        {
            var accountResult = await LoadAndValidateMainAccount(dto);
            if (!accountResult.IsSuccess)
                return await FailAndRollback(transaction, accountResult.Error!, cancellationToken);

            var account = accountResult.Result!;

            var balanceResult = UpdateAccountBalance(account, dto);
            if (!balanceResult.IsSuccess)
                return await FailAndRollback(transaction, balanceResult.Error!, cancellationToken);

            var updateResult = await _accountRepository.UpdateAsync(account);
            if (!updateResult.IsSuccess)
                return await FailAndRollback(transaction, updateResult.Error!, cancellationToken);

            var transactionEntity = CreateTransaction(dto);
            await _transactionRepository.AddAsync(transactionEntity);

            if (dto.CounterPartyAccountId is not null)
            {
                var counterPartyResult = await HandleCounterParty(dto, account);
                if (!counterPartyResult.IsSuccess)
                    return await FailAndRollback(transaction, counterPartyResult.Error!, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return MbResult<Guid>.Success(transactionEntity.Id);
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

    private async Task<MbResult<Account.Account>> LoadAndValidateMainAccount(TransactionDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(dto.AccountId);

        if (account is null)
            return MbResult<Account.Account>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт с ID {dto.AccountId} не найден"
            });

        return MbResult<Account.Account>.Success(account);
    }

    private static MbResult<Unit> UpdateAccountBalance(Account.Account account, TransactionDto dto)
    {
        if (dto.Type == TransactionType.Debit && account.Balance < dto.Amount)
            return MbResult<Unit>.Fail(new MbError
            {
                Code = "InsufficientFunds",
                Message = $"Недостаточно средств: баланс {account.Balance}, требуется {dto.Amount}"
            });

        account.Balance += dto.Type == TransactionType.Credit
            ? dto.Amount
            : -dto.Amount;

        return MbResult<Unit>.Success(Unit.Value);
    }

    private Transaction CreateTransaction(TransactionDto dto)
    {
        var transaction = _mapper.Map<Transaction>(dto);
        transaction.Id = Guid.CreateVersion7();
        transaction.Date = DateTime.UtcNow;
        return transaction;
    }

    private async Task<MbResult<Guid>> HandleCounterParty(TransactionDto dto, Account.Account account)
    {
        var counterParty = await _accountRepository.GetByIdAsync(dto.CounterPartyAccountId!.Value);
        if (counterParty is null)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "NotFound",
                Message = $"Счёт контрагента с ID {dto.CounterPartyAccountId} не найден"
            });

        if (counterParty.Currency != dto.Currency)
            return MbResult<Guid>.Fail(new MbError
            {
                Code = "CurrencyMismatch",
                Message = $"Валюта счёта {counterParty.Currency} не совпадает с валютой транзакции {dto.Currency}"
            });

        counterParty.Balance += dto.Type == TransactionType.Credit
            ? -dto.Amount
            : dto.Amount;

        var updateResult = await _accountRepository.UpdateAsync(counterParty);
        if (!updateResult.IsSuccess)
            return MbResult<Guid>.Fail(updateResult.Error!);

        var mirroredTransaction = new Transaction
        {
            Id = Guid.CreateVersion7(),
            AccountId = counterParty.Id,
            CounterPartyAccountId = account.Id,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Type = dto.Type == TransactionType.Credit
                ? TransactionType.Debit
                : TransactionType.Credit,
            Description = dto.Description,
            Date = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(mirroredTransaction);
        return MbResult<Guid>.Success(mirroredTransaction.Id);
    }

    private static async Task<MbResult<Guid>> FailAndRollback(IDbContextTransaction transaction, MbError error,
        CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken);
        return MbResult<Guid>.Fail(error);
    }
}