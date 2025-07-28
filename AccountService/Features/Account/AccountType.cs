namespace AccountService.Features.Account;

public enum AccountType
{
    /// <summary>Текущий счёт</summary>
    Checking = 0,

    /// <summary>Депозитный счёт</summary>
    Deposit = 1,

    /// <summary>Кредитный счёт</summary>
    Credit = 2
}