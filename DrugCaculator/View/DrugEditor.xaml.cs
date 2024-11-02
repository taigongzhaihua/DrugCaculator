using DrugCaculator.Models;
using DrugCaculator.Services;
using DrugCaculator.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace DrugCaculator.View
{
    public class HeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalHeight and > 85)
            {
                // 减去固定值以确保UI布局合理
                return totalHeight - 85; // 可以根据需要调整减去的值
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public partial class DrugEditor
    {
        // 构造函数
        public DrugEditor(Drug drug, DrugService drugService)
        {
            InitializeComponent();

            var viewModel = new DrugEditorViewModel(drug, drugService)
            {
                CloseAction = () => DialogResult = true // 设置关闭动作
            };
            DataContext = viewModel;
        }

        // 保存按钮的点击事件
    }
}