using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DrugCalculator.Utilities.Converters;

public class StringIsNullToVisibilityConverter : IValueConverter
{
    public static StringIsNullToVisibilityConverter Instance = new();

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}