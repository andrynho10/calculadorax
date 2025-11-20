using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using CalculadoraX.Models;
using CalculadoraX.Services;

namespace CalculadoraX.ViewModels;

public class CurrencyConverterViewModel : ObservableObject
{
    private readonly ICurrencyService _currencyService;
    private readonly IClipboardService _clipboardService;
    private readonly CultureInfo _culture = new("es-CL");

    private string _inputAmount = "1";
    private CurrencyCode _sourceCurrency = CurrencyCode.CLP;
    private CurrencyCode _targetCurrency = CurrencyCode.USD;
    private string _resultText = "—";
    private string _lastUpdated = "—";
    private string? _errorMessage;
    private bool _isBusy;
    private CurrencyConversionResult? _lastResult;
    private string? _copyFeedback;
    private readonly DispatcherTimer _feedbackTimer;

    public CurrencyConverterViewModel(ICurrencyService? currencyService = null, IClipboardService? clipboardService = null)
    {
        _currencyService = currencyService ?? new MindicadorCurrencyService();
        _clipboardService = clipboardService ?? new ClipboardService();
        AvailableCurrencies = Enum.GetValues<CurrencyCode>();
        ReferenceRates = new ObservableCollection<CurrencyDisplayRate>();

        ConvertCommand = new AsyncRelayCommand(_ => ConvertAsync());
        RefreshCommand = new AsyncRelayCommand(_ => RefreshRatesAsync(true));
        CopyResultCommand = new RelayCommand(_ => CopyResult(), _ => _lastResult is not null);
        _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _feedbackTimer.Tick += (_, _) =>
        {
            CopyFeedback = null;
            _feedbackTimer.Stop();
        };
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
        set
        {
            if (_sourceCurrency == value) return;
            var previous = _sourceCurrency;
            SetProperty(ref _sourceCurrency, value);
            if (_targetCurrency == _sourceCurrency)
            {
                TargetCurrency = previous;
            }
        }
    }

    public CurrencyCode TargetCurrency
    {
        get => _targetCurrency;
        set
        {
            if (_targetCurrency == value) return;
            var previous = _targetCurrency;
            SetProperty(ref _targetCurrency, value);
            if (_sourceCurrency == _targetCurrency)
            {
                SourceCurrency = previous;
            }
        }
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

    public string? CopyFeedback
    {
        get => _copyFeedback;
        private set => SetProperty(ref _copyFeedback, value);
    }

    public ICommand ConvertCommand { get; }
    public ICommand RefreshCommand { get; }
    public RelayCommand CopyResultCommand { get; }

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
            _lastResult = result;
            ErrorMessage = null;
            CopyResultCommand.RaiseCanExecuteChanged();
            CopyFeedback = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _lastResult = null;
            CopyResultCommand.RaiseCanExecuteChanged();
            CopyFeedback = null;
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
                : string.Format(_culture, "{0:N2}", entry.Value);
            ReferenceRates.Add(new CurrencyDisplayRate(entry.Key, display));
        }
    }

    private void CopyResult()
    {
        if (_lastResult is null) return;
        var text = _lastResult.TargetAmount.ToString("0.####", CultureInfo.InvariantCulture);
        _clipboardService.SetText(text);
        CopyFeedback = "Valor copiado";
        _feedbackTimer.Stop();
        _feedbackTimer.Start();
    }
}
