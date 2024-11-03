using DrugCaculator.Models;
using DrugCaculator.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace DrugCaculator.View.Components
{


    public partial class RuleEditorRowComponent
    {
        private readonly ConfigurationService _configService = new();
        public List<string> RouteOptions { get; set; }
        public List<string> UnitOptions { get; set; }
        public List<string> FrequencyOptions { get; set; }

        public event EventHandler CalculationRuleChanged;
        public ObservableCollection<ConditionRow> Conditions { get; set; } = [];

        public static readonly DependencyProperty CalculationRuleProperty = DependencyProperty.Register(
            nameof(CalculationRule),
            typeof(DrugCalculationRule),
            typeof(RuleEditorRowComponent),
            new FrameworkPropertyMetadata(null, OnCalculationRuleChanged));

        public DrugCalculationRule CalculationRule
        {
            get => (DrugCalculationRule)GetValue(CalculationRuleProperty);
            set => SetValue(CalculationRuleProperty, value);
        }
        private void LoadOptions()
        {
            RouteOptions = _configService.GetOption("RouteOptions");
            UnitOptions = _configService.GetOption("UnitOptions");
            FrequencyOptions = _configService.GetOption("FrequencyOptions");
        }
        public RuleEditorRowComponent()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            LoadOptions();
            ConditionsList.ItemsSource = Conditions;
            RouteComboBox.ItemsSource = RouteOptions;
            UnitComboBox.ItemsSource = UnitOptions;
            FrequencyComboBox.ItemsSource = FrequencyOptions;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(CalculationRule != null ? $"CalculationRule: {CalculationRule}" : "CalculationRule is null");
            CalculationRule ??= new DrugCalculationRule();
        }

        private static void OnCalculationRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RuleEditorRowComponent component && e.NewValue is DrugCalculationRule rule)
            {
                component.LoadConditions(EnglishToChinese(rule.Condition));
                Console.WriteLine(rule.Condition);
            }
        }

        // 添加新条件行
        private void OnAddCondition(object sender, RoutedEventArgs e)
        {
            var newCondition = new ConditionRow { ShowLogic = Conditions.Count > 0, Logic = "且" };
            Conditions.Add(newCondition);

            var conditionComponent = new RuleEditorConditionComponent();
            conditionComponent.ConditionDeleted += (s, args) => RemoveCondition(conditionComponent);
            UpdateCondition();
        }

        // 删除条件行
        public void RemoveCondition(RuleEditorConditionComponent conditionComponent)
        {
            var index = ConditionsList.Items.IndexOf(conditionComponent);
            if (index >= 0)
            {
                ConditionsList.Items.RemoveAt(index);
                Conditions.RemoveAt(index);
            }
            UpdateCondition();
        }

        private void OnConditionDeleteClicked(object sender, EventArgs e)
        {
            if (sender is RuleEditorConditionComponent conditionComponent)
            {
                Conditions.Remove(conditionComponent.ConditionRow);
                UpdateCondition();
                OnCalculationRuleChanged();
            }
        }

        private void OnLogicChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox { DataContext: ConditionRow condition, SelectedItem: ComboBoxItem selectedItem })
            {
                condition.Logic = selectedItem.Content.ToString();
                UpdateCondition();
                OnCalculationRuleChanged();
            }
        }

        private void OnFormulaChanged(object sender, TextChangedEventArgs e)
        {
            CalculationRule.Formula = ChineseToEnglish(FormulaTextBox.Text);
            OnCalculationRuleChanged();
        }

        private void OnUnitChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalculationRule != null && UnitComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                CalculationRule.Unit = selectedItem.Content.ToString();
                OnCalculationRuleChanged();
            }
        }

        private void OnFrequencyChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CalculationRule != null && FrequencyComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                CalculationRule.Frequency = selectedItem.Content.ToString();
                OnCalculationRuleChanged();
                Console.WriteLine(CalculationRule.Frequency);
            }
        }
        private void OnFormulaPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 允许输入的字符：数字、基本运算符（+ - * /）、括号和小数点
            var regex = new Regex(@"^[0-9+\-*/().]*$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        // 将英文格式转换为中文格式
        private static string EnglishToChinese(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return "";

            return Regex.Replace(
                condition.Replace(" || ", "或").Replace(" && ", "且")
                    .Replace("Age", "年龄").Replace("Weight", "体重")
                    .Replace("<", "小于").Replace(">", "大于")
                    .Replace("==", "等于").Replace("=", "等于")
                    .Replace("year", "岁").Replace("month", "月"),
                @"at \[(\d+),(\d+)\]",
                "范围在 $1-$2"
            );
        }

        // 将中文格式转换为英文格式
        private static string ChineseToEnglish(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return "";

            return Regex.Replace(
                condition.Replace("或", " || ").Replace("且", " && ")
                    .Replace("年龄", "Age").Replace("体重", "Weight")
                    .Replace("小于", "<").Replace("大于", ">")
                    .Replace("等于", "=").Replace(" = ", " == ")
                    .Replace("岁", "year").Replace("月", "month"),
                @"范围在 (\d+)\D(\d+)",
                "at [$1,$2]"
            );
        }

        // 加载条件行
        private void LoadConditions(string chineseCondition)
        {
            Conditions.Clear();

            if (string.IsNullOrWhiteSpace(chineseCondition)) return;

            var parts = Regex.Split(chineseCondition, "(且|或)").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            string logic = "";

            foreach (var part in parts)
            {
                if (part == "且" || part == "或")
                {
                    logic = part;
                }
                else
                {
                    var conditionRow = ParseCondition(part);
                    conditionRow.ShowLogic = Conditions.Count > 0;
                    conditionRow.Logic = logic;
                    Conditions.Add(conditionRow);
                    logic = ""; // 重置逻辑符
                }
            }
        }

        private ConditionRow ParseCondition(string condition)
        {
            var parts = condition.Split(' ');

            return new ConditionRow
            {
                ConditionType = parts.ElementAtOrDefault(0),
                Comparison = parts.ElementAtOrDefault(1),
                Value = parts.ElementAtOrDefault(2),
                Unit = parts.ElementAtOrDefault(3)
            };
        }

        private void UpdateCondition()
        {
            var chineseCondition = "";
            foreach (var condition in Conditions)
            {
                if (condition.ShowLogic)
                {
                    chineseCondition += (condition.Logic);
                }
                chineseCondition += condition.ToString();
            }
            Console.WriteLine(chineseCondition);
            CalculationRule.Condition = ChineseToEnglish(chineseCondition);
            Console.WriteLine(CalculationRule.Condition);
        }

        private void OnConditionChanged(object sender, EventArgs eventArgs)
        {
            UpdateCondition();
            OnCalculationRuleChanged();
        }

        protected virtual void OnCalculationRuleChanged()
        {
            CalculationRuleChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnRouteChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCondition();
            OnCalculationRuleChanged();
        }

        private void FormulaTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[0-9+\-*/(). AaeghitWw]+$");
            e.Handled = !regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text));
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
    // 条件行类

    public class ConditionRow : INotifyPropertyChanged
    {
        private string _conditionType = "年龄";
        private string _comparison = "小于";
        private string _value = "1";
        private string _unit = "岁";
        private string _logic = "且";
        private bool _showLogic;

        public string ConditionType
        {
            get => _conditionType;
            set
            {
                if (_conditionType != value)
                {
                    _conditionType = value;
                    OnPropertyChanged(nameof(ConditionType));
                }
            }
        }

        public string Comparison
        {
            get => _comparison;
            set
            {
                if (_comparison != value)
                {
                    _comparison = value;
                    OnPropertyChanged(nameof(Comparison));
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public string Unit
        {
            get => _unit;
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged(nameof(Unit));
                }
            }
        }

        public string Logic
        {
            get => _logic;
            set
            {
                if (_logic != value)
                {
                    _logic = value;
                    OnPropertyChanged(nameof(Logic));
                }
            }
        }

        public bool ShowLogic
        {
            get => _showLogic;
            set
            {
                if (_showLogic != value)
                {
                    _showLogic = value;
                    OnPropertyChanged(nameof(ShowLogic));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{ConditionType} {Comparison} {Value} {Unit}".Trim();
        }
    }
}