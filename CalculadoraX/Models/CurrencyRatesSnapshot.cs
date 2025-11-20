namespace CalculadoraX.Models;

public record CurrencyRatesSnapshot(
    DateTime RetrievedAt,
    IReadOnlyDictionary<CurrencyCode, decimal> RatesToClp);
