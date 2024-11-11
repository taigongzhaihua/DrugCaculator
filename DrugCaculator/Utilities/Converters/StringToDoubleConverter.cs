using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace DrugCaculator.Utilities.Converters;

public class StringToDoubleConverter : IValueConverter
{
    private string _value;
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (_value != null && Regex.IsMatch(_value, @"^\d+\.$"))
        {
            return _value;
        }

        return value?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        _value = value?.ToString();
        return double.TryParse(value?.ToString(), out var result) ? result : DependencyProperty.UnsetValue; // 返回未设置的值以避免异常
    }
}