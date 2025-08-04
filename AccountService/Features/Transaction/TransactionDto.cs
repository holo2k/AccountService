namespace AccountService.Features.Transaction;

/// <summary>
///     DTO, представляющее транзакцию по счёту.
/// </summary>
public class TransactionDto
{
    /// <summary>
    ///     Уникальный идентификатор транзакции.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid Id { get; set; }

    /// <summary>
    ///     Идентификатор счёта, к которому относится транзакция.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    ///     Идентификатор счёта контрагента (опционально).
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public Guid? CounterPartyAccountId { get; set; }

    /// <summary>
    ///     Сумма транзакции.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Валюта транзакции в формате ISO 4217.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Тип транзакции:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Credit — зачисление</description>
    ///         </item>
    ///         <item>
    ///             <description>Debit — списание</description>
    ///         </item>
    ///     </list>
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    ///     Описание транзакции.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Дата и время совершения транзакции.
    /// </summary>
    // ReSharper disable once UnusedMember.Global (Используется в auto mapper)
    public DateTime Date { get; set; }
}