using CalculadoraX.Models;

namespace CalculadoraX.Services;

public interface ICurrencyService
{
    Task<CurrencyConversionResult> ConvertAsync(decimal amount, CurrencyCode source, CurrencyCode target, bool forceRefresh = false, CancellationToken cancellationToken = default);
    Task<CurrencyRatesSnapshot> GetRatesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default);
}
