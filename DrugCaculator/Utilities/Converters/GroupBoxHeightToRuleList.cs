using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugCaculator.Utilities.Converters;

public class GroupBoxHeightToRuleList : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double totalHeight and > 85)
        {
            // 减去固定值以确保UI布局合理
            return totalHeight - 70; // 可以根据需要调整减去的值
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}