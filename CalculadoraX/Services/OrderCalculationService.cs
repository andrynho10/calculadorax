using CalculadoraX.Models;

namespace CalculadoraX.Services;

public class OrderCalculationService
{
    private const decimal DefaultTaxRate = 0.19m;

    public OrderCalculationResult Calculate(decimal amount, InputMode mode, decimal? customRate = null)
    {
        var rate = customRate ?? DefaultTaxRate;
        if (rate <= 0 || rate >= 1) throw new InvalidOperationException("La tasa debe ser mayor a 0 y menor a 1.");
        if (amount <= 0) throw new InvalidOperationException("El monto debe ser mayor que cero.");

        return mode switch
        {
            InputMode.Gross => FromGross(amount, rate),
            InputMode.Net => FromNet(amount, rate),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static OrderCalculationResult FromGross(decimal gross, decimal rate)
    {
        var net = Math.Round(gross / (1 + rate), 2, MidpointRounding.AwayFromZero);
        var tax = gross - net;
        return new OrderCalculationResult(gross, net, tax, rate);
    }

    private static OrderCalculationResult FromNet(decimal net, decimal rate)
    {
        var gross = Math.Round(net * (1 + rate), 2, MidpointRounding.AwayFromZero);
        var tax = gross - net;
        return new OrderCalculationResult(gross, net, tax, rate);
    }
}
