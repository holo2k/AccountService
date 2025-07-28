namespace AccountService.Exceptions;

public class AccountNotFoundException : Exception
{
    public AccountNotFoundException(Guid accountId)
        : base($"Счёт с ID {accountId} не найден")
    {
    }
}