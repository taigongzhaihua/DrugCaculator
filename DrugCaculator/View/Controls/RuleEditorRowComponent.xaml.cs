using DrugCalculator.Models;
using DrugCalculator.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCalculator.View.Controls;

public partial class RuleEditorRowComponent
{
    private readonly ConfigurationService _configService = ConfigurationService.Instance;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    public List<string> LogicOptions { get; set; }
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
        LogicOptions = _configService.GetOption("LogicOptions");
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
        _logger.Debug(CalculationRule != null ? $"CalculationRule: {CalculationRule}" : "CalculationRule is null");
        CalculationRule ??= new DrugCalculationRule();
    }

    private static void OnCalculationRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RuleEditorRowComponent component || e.NewValue is not DrugCalculationRule rule) return;
        component.LoadConditions(EnglishToChinese(rule.Condition));
        Console.WriteLine(rule.Condition);
    }

    // 添加新条件行
    private void OnAddCondition(object sender, RoutedEventArgs e)
    {
        var newCondition = new ConditionRow { ShowLogic = Conditions.Count > 0, Logic = "且" };
        Conditions.Add(newCondition);

        var conditionComponent = new RuleEditorConditionComponent();
        conditionComponent.ConditionDeleted += (_, _) => RemoveCondition(conditionComponent);
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
        if (sender is not RuleEditorConditionComponent conditionComponent) return;
        Conditions.Remove(conditionComponent.ConditionRow);
        UpdateCondition();
        OnCalculationRuleChanged();
    }

    private void OnLogicChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox
            {
                DataContext: ConditionRow condition, SelectedItem: ComboBoxItem selectedItem
            }) return;
        condition.Logic = selectedItem.Content.ToString();
        UpdateCondition();
        OnCalculationRuleChanged();
    }

    private void OnFormulaChanged(object sender, TextChangedEventArgs e)
    {
        CalculationRule.Formula = ChineseToEnglish(FormulaTextBox.Text);
        OnCalculationRuleChanged();
    }

    private void OnUnitChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CalculationRule == null || UnitComboBox?.SelectedItem is not ComboBoxItem selectedItem) return;
        CalculationRule.Unit = selectedItem.Content.ToString();
        OnCalculationRuleChanged();
    }

    private void OnFrequencyChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CalculationRule == null || FrequencyComboBox?.SelectedItem is not ComboBoxItem selectedItem) return;
        CalculationRule.Frequency = selectedItem.Content.ToString();
        OnCalculationRuleChanged();
        Console.WriteLine(CalculationRule.Frequency);
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
        var logic = "";

        foreach (var part in parts)
            if (part is "且" or "或")
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

    private static ConditionRow ParseCondition(string condition)
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
            if (condition.ShowLogic) chineseCondition += condition.Logic;
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