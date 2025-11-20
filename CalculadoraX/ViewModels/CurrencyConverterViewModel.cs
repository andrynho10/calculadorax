using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using CalculadoraX.Models;
using CalculadoraX.Services;

namespace CalculadoraX.ViewModels;

public class CurrencyConverterViewModel : ObservableObject
{
    private readonly ICurrencyService _currencyService;
    private readonly CultureInfo _culture = new("es-CL");

    private string _inputAmount = "1";
    private CurrencyCode _sourceCurrency = CurrencyCode.CLP;
    private CurrencyCode _targetCurrency = CurrencyCode.USD;
    private string _resultText = "—";
    private string _lastUpdated = "—";
    private string? _errorMessage;
    private bool _isBusy;

    public CurrencyConverterViewModel(ICurrencyService? currencyService = null)
    {
        _currencyService = currencyService ?? new MindicadorCurrencyService();
        AvailableCurrencies = Enum.GetValues<CurrencyCode>();
        ReferenceRates = new ObservableCollection<CurrencyDisplayRate>();

        ConvertCommand = new AsyncRelayCommand(_ => ConvertAsync());
        RefreshCommand = new AsyncRelayCommand(_ => RefreshRatesAsync(true));
    }

    public Array AvailableCurrencies { get; }
    public ObservableCollection<CurrencyDisplayRate> ReferenceRates { get; }

    public string InputAmount
    {
        get => _inputAmount;
        set => SetProperty(ref _inputAmount, value);
    }

    public CurrencyCode SourceCurrency
    {
        get => _sourceCurrency;
        set => SetProperty(ref _sourceCurrency, value);
    }

    public CurrencyCode TargetCurrency
    {
        get => _targetCurrency;
        set => SetProperty(ref _targetCurrency, value);
    }

    public string ResultText
    {
        get => _resultText;
        private set => SetProperty(ref _resultText, value);
    }

    public string LastUpdated
    {
        get => _lastUpdated;
        private set => SetProperty(ref _lastUpdated, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public ICommand ConvertCommand { get; }
    public ICommand RefreshCommand { get; }

    public async Task InitializeAsync()
    {
        await RefreshRatesAsync(false);
    }

    private async Task ConvertAsync()
    {
        if (!decimal.TryParse(InputAmount, NumberStyles.Number, _culture, out var amount))
        {
            ErrorMessage = "Ingresa un monto válido.";
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _currencyService.ConvertAsync(amount, SourceCurrency, TargetCurrency, false);
            ResultText = string.Format(_culture, "{0:N4}", result.TargetAmount);
            LastUpdated = result.RetrievedAt.ToLocalTime().ToString("g", _culture);
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshRatesAsync(bool forceRefresh)
    {
        try
        {
            IsBusy = true;
            var snapshot = await _currencyService.GetRatesAsync(forceRefresh);
            UpdateReferenceRates(snapshot);
            LastUpdated = snapshot.RetrievedAt.ToLocalTime().ToString("g", _culture);
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateReferenceRates(CurrencyRatesSnapshot snapshot)
    {
        ReferenceRates.Clear();
        foreach (var entry in snapshot.RatesToClp)
        {
            var display = entry.Key == CurrencyCode.CLP
                ? "1"
                : string.Format(_culture, "{0:N4}", entry.Value);
            ReferenceRates.Add(new CurrencyDisplayRate(entry.Key, display));
        }
    }
}
