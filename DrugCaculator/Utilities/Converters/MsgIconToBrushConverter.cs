using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace DrugCalculator.Utilities.Converters;

public class MsgIconToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            "\ue615" => Brushes.DodgerBlue,
            "\ue629" => Brushes.DarkOrange,
            "\ue60b" => Brushes.IndianRed,
            "\ue665" => Brushes.ForestGreen,
            _ => Brushes.Black
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Brush) return DependencyProperty.UnsetValue;

        if (value.Equals(Brushes.DodgerBlue)) return "\ue615";
        if (value.Equals(Brushes.DarkOrange)) return "\ue629";
        if (value.Equals(Brushes.IndianRed)) return "\ue60b";
        if (value.Equals(Brushes.ForestGreen)) return "\ue665";
        if (value.Equals(Brushes.Black)) return null;
        return null;
    }
}