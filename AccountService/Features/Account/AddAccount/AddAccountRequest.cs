namespace AccountService.Features.Account.AddAccount;

/// <summary>
///     Запрос на добавление банковского счёта.
/// </summary>
public class AddAccountRequest
{
    /// <summary>
    ///     Идентификатор владельца счёта.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    ///     Тип счёта
    /// </summary>
    public AccountType Type { get; set; }

    /// <summary>
    ///     Валюта счёта в формате ISO 4217 (например, USD, EUR).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Текущий баланс счёта.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Процентная ставка по счёту (может быть null).
    /// </summary>
    public decimal? PercentageRate { get; set; }
}