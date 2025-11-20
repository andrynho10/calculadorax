using CalculadoraX.Services;

namespace CalculadoraX.ViewModels;

public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        var currencyService = new MindicadorCurrencyService();

        OrderViewModel = new OrderCalculationViewModel(new OrderCalculationService());
        HonorariumViewModel = new HonorariumCalculationViewModel(new HonorariumCalculationService());
        CurrencyViewModel = new CurrencyConverterViewModel(currencyService);
    }

    public OrderCalculationViewModel OrderViewModel { get; }
    public HonorariumCalculationViewModel HonorariumViewModel { get; }
    public CurrencyConverterViewModel CurrencyViewModel { get; }

    public async Task InitializeAsync()
    {
        await CurrencyViewModel.InitializeAsync();
    }
}
