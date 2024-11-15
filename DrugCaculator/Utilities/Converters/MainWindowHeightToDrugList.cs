using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugCalculator.Utilities.Converters;

public class MainWindowHeightToDrugList : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double totalHeight)
            // 减去固定值以确保UI布局合理
            return totalHeight >= 165 ? totalHeight - 165 : 0; // 可以根据需要调整减去的值
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}