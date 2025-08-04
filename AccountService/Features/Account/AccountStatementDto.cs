using AccountService.Features.Transaction;

namespace AccountService.Features.Account;

/// <summary>
///     DTO, представляющее выписку по счёту с перечнем транзакций.
/// </summary>
public class AccountStatementDto
{
    /// <summary>
    ///     Уникальный идентификатор счёта.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Уникальный идентификатор владельца счёта.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid OwnerId { get; set; }

    /// <summary>
    ///     Валюта счёта в формате ISO 4217.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Тип счёта (расчётный, депозитный, кредитный).
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public AccountType Type { get; set; }

    /// <summary>
    ///     Текущий баланс счёта.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public decimal Balance { get; set; }

    /// <summary>
    ///     Список транзакций по счёту.
    /// </summary>
    public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
}