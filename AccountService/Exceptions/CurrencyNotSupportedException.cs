namespace AccountService.Exceptions;

public class CurrencyNotSupportedException : Exception
{
    public CurrencyNotSupportedException(string currency)
        : base($"Неподдерживаемая валюта: {currency}")
    {
    }
}