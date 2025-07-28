namespace AccountService.CurrencyService.Abstractions;

public interface ICurrencyService
{
    bool IsSupported(string currencyCode);
}