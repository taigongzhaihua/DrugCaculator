using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugCaculator.Utilities.Converters;

public class IsCloseToBoolOrNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && parameter is string targetValue)
        {
            return stringValue == targetValue;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true && parameter is string targetValue)
        {
            return targetValue;
        }
        return Binding.DoNothing;
    }
}