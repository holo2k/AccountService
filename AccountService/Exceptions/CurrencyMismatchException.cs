namespace AccountService.Exceptions;

public class CurrencyMismatchException : Exception
{
    public CurrencyMismatchException()
        : base("Не совпадение валют")
    {
    }
}