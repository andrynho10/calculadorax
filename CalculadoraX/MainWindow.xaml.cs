using System.Windows;
using CalculadoraX.ViewModels;

namespace CalculadoraX;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (DataContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
