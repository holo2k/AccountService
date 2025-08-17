namespace AccountService.Features.Account;

/// <summary>
///     DTO, представляющее банковский счёт.
/// </summary>
public class AccountDto
{
    /// <summary>
    ///     Уникальный идентификатор счёта.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Уникальный идентификатор владельца счёта.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    ///     Тип счёта:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Checking — Текущий счёт</description>
    ///         </item>
    ///         <item>
    ///             <description>Deposit — Депозитный счёт</description>
    ///         </item>
    ///         <item>
    ///             <description>Credit — Кредитный счёт</description>
    ///         </item>
    ///     </list>
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    ///     Валюта счёта в формате ISO 4217 (например, "USD", "EUR").
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий баланс счёта.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public decimal Balance { get; set; }

    /// <summary>
    ///     Процентная ставка (опционально).
    /// </summary>
    public decimal? PercentageRate { get; set; }

    /// <summary>
    ///     Дата открытия счёта.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime OpenDate { get; set; }

    /// <summary>
    ///     Дата закрытия счёта (опционально).
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime? CloseDate { get; set; }

    /// <summary>
    ///     Флаг заморозки счёта.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public bool IsFrozen { get; set; }
}