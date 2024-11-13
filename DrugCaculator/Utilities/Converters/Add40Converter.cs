using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugCalculator.Utilities.Converters
{
    // 静态转换器实例以减少重复创建，转换器用于在绑定时增加一个固定值（40）
    public class Add40Converter : IValueConverter
    {
        public static Add40Converter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double doubleValue ? doubleValue + 40 : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double doubleValue ? doubleValue - 40 : value;
    }

}
