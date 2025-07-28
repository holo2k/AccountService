namespace AccountService.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(Guid accountId, decimal currentBalance, decimal required)
        : base($"Недостаточно средств на аккаунте {accountId}. Баланс: {currentBalance}, требуется: {required}")
    {
    }
}