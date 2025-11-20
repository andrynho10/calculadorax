using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CalculadoraX.Models;

namespace CalculadoraX.Services;

public class MindicadorCurrencyService : ICurrencyService
{
    private const string Endpoint = "https://mindicador.cl/api";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    private readonly HttpClient _httpClient;
    private readonly LocalCurrencyCache _cache;

    public MindicadorCurrencyService(HttpClient? httpClient = null, LocalCurrencyCache? cache = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _cache = cache ?? new LocalCurrencyCache();
    }

    public async Task<CurrencyConversionResult> ConvertAsync(decimal amount, CurrencyCode source, CurrencyCode target, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new InvalidOperationException("El monto debe ser positivo.");
        var snapshot = await GetRatesAsync(forceRefresh, cancellationToken);
        var sourceRate = GetRate(snapshot, source);
        var targetRate = GetRate(snapshot, target);

        decimal amountInClp = source == CurrencyCode.CLP ? amount : amount * sourceRate;
        decimal targetAmount = target == CurrencyCode.CLP ? amountInClp : amountInClp / targetRate;
        targetAmount = Math.Round(targetAmount, 4, MidpointRounding.AwayFromZero);

        return new CurrencyConversionResult(source, target, amount, targetAmount, snapshot.RetrievedAt);
    }

    public async Task<CurrencyRatesSnapshot> GetRatesAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        if (!forceRefresh)
        {
            var cached = await _cache.LoadAsync(cancellationToken);
            if (cached is not null && DateTime.UtcNow - cached.RetrievedAt <= CacheDuration)
            {
                return cached;
            }
        }

        var fetched = await FetchRemoteAsync(cancellationToken);
        await _cache.SaveAsync(fetched, cancellationToken);
        return fetched;
    }

    private async Task<CurrencyRatesSnapshot> FetchRemoteAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<MindicadorResponse>(Endpoint, cancellationToken);
        if (response is null)
        {
            throw new InvalidOperationException("No fue posible obtener valores de la API.");
        }

        var retrievedAt = response.Fecha ?? DateTime.UtcNow;
        var dict = new Dictionary<CurrencyCode, decimal>
        {
            [CurrencyCode.CLP] = 1m,
            [CurrencyCode.UF] = response.Uf?.Valor ?? throw new InvalidOperationException("La respuesta no incluye UF."),
            [CurrencyCode.USD] = response.Dolar?.Valor ?? throw new InvalidOperationException("La respuesta no incluye USD."),
            [CurrencyCode.EUR] = response.Euro?.Valor ?? throw new InvalidOperationException("La respuesta no incluye EUR.")
        };

        return new CurrencyRatesSnapshot(retrievedAt, dict);
    }

    private static decimal GetRate(CurrencyRatesSnapshot snapshot, CurrencyCode code)
    {
        return snapshot.RatesToClp.TryGetValue(code, out var value)
            ? value
            : code == CurrencyCode.CLP ? 1m : throw new InvalidOperationException($"No hay tasa configurada para {code}.");
    }

    private class MindicadorResponse
    {
        [JsonPropertyName("fecha")]
        public DateTime? Fecha { get; set; }

        [JsonPropertyName("uf")]
        public Indicator? Uf { get; set; }

        [JsonPropertyName("dolar")]
        public Indicator? Dolar { get; set; }

        [JsonPropertyName("euro")]
        public Indicator? Euro { get; set; }
    }

    private class Indicator
    {
        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }
    }
}
