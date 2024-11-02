using DrugCaculator.ViewModels;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DrugCaculator.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public class HeightMinusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalHeight)
            {
                // 减去固定值以确保UI布局合理
                return totalHeight - 180; // 可以根据需要调整减去的值
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Regex.IsMatch(value?.ToString()!, $@"\D"))
            {
                value = Regex.Replace(value?.ToString()!, $@"\D", "");
            }
            return int.TryParse(value?.ToString(), out var result) ? result : DependencyProperty.UnsetValue; // 返回未设置的值以避免异常
        }
    }
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 订阅 ViewModel 的 PropertyChanged 事件
            if (DataContext is DrugViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedDrug") return;
            // 当 SelectedDrug 改变时，让 WeightTextBox 获得焦点
            WeightTextBox.Focus();
            if (WeightTextBox.Text == "0")
            {
                WeightTextBox.Text = "";
            }
        }
        private void WeightTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Enter or Key.Tab or Key.Space)) return;
            // 当按下回车键时，将焦点设置到 AgeTextBox
            AgeTextBox.Focus();
            if (AgeTextBox.Text == "0")
            {
                AgeTextBox.Text = "";
            }
            if (WeightTextBox.Text == "")
            {
                WeightTextBox.Text = "0";
            }
        }
        private void AgeTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key is not (Key.Enter or Key.Tab or Key.Space)) return;
            // 当按下回车键时，将焦点设置到 AgeTextBox
            AgeUnitComboBox.IsDropDownOpen = true;
            AgeUnitComboBox.Focus();
            if (AgeTextBox.Text == "")
            {
                AgeTextBox.Text = "0";
            }
        }
        private void AgeUnitComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (AgeUnitComboBox.IsDropDownOpen == false)
                    {
                        AgeUnitComboBox.IsDropDownOpen = !AgeUnitComboBox.IsDropDownOpen;
                    }
                    break;
                case Key.Down or Key.Space:
                    AgeUnitComboBox.IsDropDownOpen = true;
                    break;
                case Key.Tab:
                    {
                        if (WeightTextBox.Text == "0")
                        {
                            WeightTextBox.Text = "";
                        }
                        WeightTextBox.Focus();
                        break;
                    }

            }
        }
        private void AgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 允许数字和一个小数点
            var regex = new Regex(@"^[0-9]*");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text));
        }
        private void WeightTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 允许数字和一个小数点
            var regex = new Regex(@"^[0-9]*(?:\.[0-9]*)?$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text));
        }



    }
}
