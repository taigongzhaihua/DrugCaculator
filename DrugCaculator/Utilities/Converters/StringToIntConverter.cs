using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace DrugCalculator.Utilities.Converters;

public class StringToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Regex.IsMatch(value?.ToString()!, @"\D"))
        {
            value = Regex.Replace(value?.ToString()!, @"\D", "");
        }
        return int.TryParse(value?.ToString(), out var result) ? result : DependencyProperty.UnsetValue; // 返回未设置的值以避免异常
    }
}