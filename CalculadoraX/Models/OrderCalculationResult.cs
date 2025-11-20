namespace CalculadoraX.Models;

public record OrderCalculationResult(
    decimal GrossAmount,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TaxRate);
