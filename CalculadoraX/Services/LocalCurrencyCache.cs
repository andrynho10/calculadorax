using System.IO;
using System.Text.Json;
using CalculadoraX.Models;

namespace CalculadoraX.Services;

public class LocalCurrencyCache
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public LocalCurrencyCache(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(GetAppDataFolder(), "currency-cache.json");
    }

    public async Task SaveAsync(CurrencyRatesSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var dto = new SerializableSnapshot
        {
            RetrievedAt = snapshot.RetrievedAt,
            Rates = snapshot.RatesToClp.ToDictionary(k => k.Key.ToString(), v => v.Value)
        };

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, dto, Options, cancellationToken);
    }

    public async Task<CurrencyRatesSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath)) return null;
        await using var stream = File.OpenRead(_filePath);
        var dto = await JsonSerializer.DeserializeAsync<SerializableSnapshot>(stream, Options, cancellationToken);
        if (dto is null || dto.Rates is null) return null;

        var dict = dto.Rates
            .Select(kvp => (Enum.Parse<CurrencyCode>(kvp.Key), kvp.Value))
            .ToDictionary(k => k.Item1, v => v.Value);

        return new CurrencyRatesSnapshot(dto.RetrievedAt, dict);
    }

    private static string GetAppDataFolder()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, "CalculadoraX");
    }

    private class SerializableSnapshot
    {
        public DateTime RetrievedAt { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}
