using AccountService.CurrencyService.Abstractions;

namespace AccountService.CurrencyService.Implementations;

public class CurrencyService : ICurrencyService
{
    private static readonly HashSet<string> SupportedCurrencies = new()
    {
        "RUB", "USD", "EUR"
    };

    public bool IsSupported(string currencyCode)
    {
        return SupportedCurrencies.Contains(currencyCode.ToUpperInvariant());
    }
}