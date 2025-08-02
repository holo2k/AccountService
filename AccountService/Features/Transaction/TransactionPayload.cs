namespace AccountService.Features.Transaction;

/// <summary>
///     Модель полезной нагрузки для создания транзакции между счетами.
/// </summary>
public class TransactionPayload
{
    /// <summary>
    ///     Идентификатор счёта отправителя средств.
    /// </summary>
    public Guid FromAccountId { get; set; }

    /// <summary>
    ///     Идентификатор счёта получателя средств.
    /// </summary>
    public Guid ToAccountId { get; set; }

    /// <summary>
    ///     Сумма перевода.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    ///     Валюта перевода в формате ISO 4217 (например, "USD", "EUR").
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Описание или назначение платежа.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}