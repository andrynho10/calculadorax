using System;
using System.Globalization;
using System.Windows.Data;
using CalculadoraX.Models;

namespace CalculadoraX.Converters;

public class InputModeToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not InputMode mode || parameter is not string param) return false;
        if (!Enum.TryParse<InputMode>(param, out var desired)) return false;
        return mode == desired;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string param || !Enum.TryParse<InputMode>(param, out var desired))
        {
            return Binding.DoNothing;
        }

        return Equals(value, true) ? desired : Binding.DoNothing;
    }
}
