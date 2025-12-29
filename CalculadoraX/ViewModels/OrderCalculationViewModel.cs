using System.Globalization;
using System.Windows.Input;
using System.Windows.Threading;
using CalculadoraX.Models;
using CalculadoraX.Services;
using ModelInputMode = CalculadoraX.Models.InputMode;

namespace CalculadoraX.ViewModels;

public class OrderCalculationViewModel : ObservableObject
{
    private readonly OrderCalculationService _service;
    private readonly IClipboardService _clipboardService;
    private readonly CultureInfo _culture = new("es-CL");

    private string _inputValue = string.Empty;
    private ModelInputMode _selectedMode = ModelInputMode.Net;
    private string _grossText = "—";
    private string _netText = "—";
    private string _taxText = "—";
    private string? _errorMessage;
    private OrderCalculationResult? _lastResult;
    private string? _copyFeedback;
    private readonly DispatcherTimer _feedbackTimer;

    public OrderCalculationViewModel(OrderCalculationService? service = null, IClipboardService? clipboardService = null)
    {
        _service = service ?? new OrderCalculationService();
        _clipboardService = clipboardService ?? new ClipboardService();
        CalculateCommand = new RelayCommand(_ => Calculate());
        ClearCommand = new RelayCommand(_ => Clear());
        CopyGrossCommand = new RelayCommand(_ => CopyAmount(_lastResult?.GrossAmount), _ => _lastResult is not null);
        CopyNetCommand = new RelayCommand(_ => CopyAmount(_lastResult?.NetAmount), _ => _lastResult is not null);
        CopyTaxCommand = new RelayCommand(_ => CopyAmount(_lastResult?.TaxAmount), _ => _lastResult is not null);
        _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _feedbackTimer.Tick += (_, _) =>
        {
            CopyFeedback = null;
            _feedbackTimer.Stop();
        };
    }

    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }

    public ModelInputMode SelectedMode
    {
        get => _selectedMode;
        set => SetProperty(ref _selectedMode, value);
    }

    public string GrossText
    {
        get => _grossText;
        private set => SetProperty(ref _grossText, value);
    }

    public string NetText
    {
        get => _netText;
        private set => SetProperty(ref _netText, value);
    }

    public string TaxText
    {
        get => _taxText;
        private set => SetProperty(ref _taxText, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string? CopyFeedback
    {
        get => _copyFeedback;
        private set => SetProperty(ref _copyFeedback, value);
    }

    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }
    public RelayCommand CopyGrossCommand { get; }
    public RelayCommand CopyNetCommand { get; }
    public RelayCommand CopyTaxCommand { get; }

    private void Calculate()
    {
        if (!TryParseInput(out var value))
        {
            ErrorMessage = "Ingresa un monto válido.";
            return;
        }

        try
        {
            var result = _service.Calculate(value, SelectedMode);
            GrossText = FormatCurrency(result.GrossAmount);
            NetText = FormatCurrency(result.NetAmount);
            TaxText = FormatCurrency(result.TaxAmount);
             _lastResult = result;
            ErrorMessage = null;
            UpdateCopyCommands();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            _lastResult = null;
            UpdateCopyCommands();
        }
    }

    private void Clear()
    {
        InputValue = string.Empty;
        GrossText = NetText = TaxText = "—";
        ErrorMessage = null;
        _lastResult = null;
        UpdateCopyCommands();
        CopyFeedback = null;
    }

    private string FormatCurrency(decimal value) => string.Format(_culture, "{0:N2}", value);

    private bool TryParseInput(out decimal value)
    {
        var raw = InputValue ?? string.Empty;
        var sanitized = raw.Replace(".", string.Empty)
            .Replace(" ", string.Empty);
        return decimal.TryParse(sanitized, NumberStyles.Number, _culture, out value);
    }

    private void CopyAmount(decimal? amount)
    {
        if (amount is null) return;
        var text = amount.Value.ToString("0.##", _culture);
        _clipboardService.SetText(text);
        CopyFeedback = "Monto copiado";
        _feedbackTimer.Stop();
        _feedbackTimer.Start();
    }

    private void UpdateCopyCommands()
    {
        CopyGrossCommand.RaiseCanExecuteChanged();
        CopyNetCommand.RaiseCanExecuteChanged();
        CopyTaxCommand.RaiseCanExecuteChanged();
    }
}
