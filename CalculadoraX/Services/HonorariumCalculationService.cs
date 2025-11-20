using CalculadoraX.Models;

namespace CalculadoraX.Services;

public class HonorariumCalculationService
{
    private const decimal DefaultRetentionRate = 0.145m;

    public HonorariumCalculationResult Calculate(decimal amount, InputMode mode, decimal? customRate = null)
    {
        var rate = customRate ?? DefaultRetentionRate;
        if (rate <= 0 || rate >= 1) throw new InvalidOperationException("La tasa debe ser mayor a 0 y menor a 1.");
        if (amount <= 0) throw new InvalidOperationException("El monto debe ser mayor que cero.");

        return mode switch
        {
            InputMode.Gross => FromGross(amount, rate),
            InputMode.Net => FromNet(amount, rate),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static HonorariumCalculationResult FromGross(decimal gross, decimal rate)
    {
        var retention = Math.Round(gross * rate, 2, MidpointRounding.AwayFromZero);
        var net = gross - retention;
        return new HonorariumCalculationResult(gross, net, retention, rate);
    }

    private static HonorariumCalculationResult FromNet(decimal net, decimal rate)
    {
        var gross = Math.Round(net / (1 - rate), 2, MidpointRounding.AwayFromZero);
        var retention = gross - net;
        return new HonorariumCalculationResult(gross, net, retention, rate);
    }
}
