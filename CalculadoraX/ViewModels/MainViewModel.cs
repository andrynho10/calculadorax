using CalculadoraX.Services;

namespace CalculadoraX.ViewModels;

public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        var currencyService = new MindicadorCurrencyService();
        var clipboardService = new ClipboardService();

        OrderViewModel = new OrderCalculationViewModel(new OrderCalculationService(), clipboardService);
        HonorariumViewModel = new HonorariumCalculationViewModel(new HonorariumCalculationService(), clipboardService);
        CurrencyViewModel = new CurrencyConverterViewModel(currencyService, clipboardService);
    }

    public OrderCalculationViewModel OrderViewModel { get; }
    public HonorariumCalculationViewModel HonorariumViewModel { get; }
    public CurrencyConverterViewModel CurrencyViewModel { get; }

    public async Task InitializeAsync()
    {
        await CurrencyViewModel.InitializeAsync();
    }
}
