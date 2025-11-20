namespace CalculadoraX.Models;

public record HonorariumCalculationResult(
    decimal GrossAmount,
    decimal NetAmount,
    decimal RetentionAmount,
    decimal RetentionRate);
