namespace CalculadoraX.Models;

public record CurrencyConversionResult(
    CurrencyCode Source,
    CurrencyCode Target,
    decimal SourceAmount,
    decimal TargetAmount,
    DateTime RetrievedAt);
