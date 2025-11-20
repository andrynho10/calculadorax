using System.Globalization;
using System.Windows.Input;
using CalculadoraX.Models;
using CalculadoraX.Services;

namespace CalculadoraX.ViewModels;

public class HonorariumCalculationViewModel : ObservableObject
{
    private readonly HonorariumCalculationService _service;
    private readonly CultureInfo _culture = new("es-CL");

    private string _inputValue = string.Empty;
    private InputMode _selectedMode = InputMode.Gross;
    private string _grossText = "—";
    private string _netText = "—";
    private string _retentionText = "—";
    private string? _errorMessage;

    public HonorariumCalculationViewModel(HonorariumCalculationService? service = null)
    {
        _service = service ?? new HonorariumCalculationService();
        CalculateCommand = new RelayCommand(_ => Calculate());
        ClearCommand = new RelayCommand(_ => Clear());
    }

    public string InputValue
    {
        get => _inputValue;
        set => SetProperty(ref _inputValue, value);
    }

    public InputMode SelectedMode
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

    public string RetentionText
    {
        get => _retentionText;
        private set => SetProperty(ref _retentionText, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ICommand CalculateCommand { get; }
    public ICommand ClearCommand { get; }

    private void Calculate()
    {
        if (!decimal.TryParse(InputValue, NumberStyles.Number, _culture, out var value))
        {
            ErrorMessage = "Ingresa un monto válido.";
            return;
        }

        try
        {
            var result = _service.Calculate(value, SelectedMode);
            GrossText = FormatCurrency(result.GrossAmount);
            NetText = FormatCurrency(result.NetAmount);
            RetentionText = FormatCurrency(result.RetentionAmount);
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void Clear()
    {
        InputValue = string.Empty;
        GrossText = NetText = RetentionText = "—";
        ErrorMessage = null;
    }

    private string FormatCurrency(decimal value) => string.Format(_culture, "{0:C0}", value);
}
