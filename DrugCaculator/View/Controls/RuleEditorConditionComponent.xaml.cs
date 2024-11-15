using DrugCalculator.Models;
using DrugCalculator.Services;
using DrugCalculator.View.Components;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DrugCalculator.View.Controls;

public partial class RuleEditorConditionComponent
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // 删除事件
    public event EventHandler ConditionDeleted; // 删除条件事件
    public event EventHandler ConditionChanged; // 条件变化事件
    private readonly ConfigurationService _configurationService = ConfigurationService.Instance; // 配置服务单例

    public List<string> ConditionTypeOptions { get; private set; } // 条件类型选项列表
    public List<string> ConditionUnitOptions { get; private set; } // 条件单位选项列表
    public List<string> ComparisonOptions { get; private set; } // 比较选项列表

    public RuleEditorConditionComponent()
    {
        try
        {
            InitializeComponent();
            LoadOptions(); // 加载选项
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化 RuleEditorConditionComponent 时发生错误。");
            CustomMessageBox.Show("初始化组件时发生错误，请检查日志以获取详细信息。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    // 定义 IsDeleteVisible 依赖属性，控制删除按钮的可见性
    public static readonly DependencyProperty IsDeleteVisibleProperty =
        DependencyProperty.Register(
            nameof(IsDeleteVisible),
            typeof(bool),
            typeof(RuleEditorConditionComponent),
            new PropertyMetadata(true, OnIsDeleteVisibleChanged));

    public bool IsDeleteVisible
    {
        get => (bool)GetValue(IsDeleteVisibleProperty);
        set => SetValue(IsDeleteVisibleProperty, value);
    }

    // 定义 ConditionRow 依赖属性，绑定条件行数据
    public static readonly DependencyProperty ConditionRowProperty =
        DependencyProperty.Register(
            nameof(ConditionRow),
            typeof(ConditionRow),
            typeof(RuleEditorConditionComponent),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, // 支持双向绑定
                OnConditionRowChanged));

    public ConditionRow ConditionRow
    {
        get => (ConditionRow)GetValue(ConditionRowProperty);
        set => SetValue(ConditionRowProperty, value);
    }

    // 加载选项数据
    private void LoadOptions()
    {
        try
        {
            ConditionTypeOptions = _configurationService.GetOption("ConditionTypeOptions");
            ConditionUnitOptions = _configurationService.GetOption("ConditionUnitOptions");
            ComparisonOptions = _configurationService.GetOption("ComparisonOptions");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载选项时发生错误。");
            CustomMessageBox.Show("加载选项时发生错误，请检查日志以获取详细信息。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    // 更新删除按钮的可见性
    private static void OnIsDeleteVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        try
        {
            if (d is RuleEditorConditionComponent component)
            {
                component.DeleteButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "更新删除按钮的可见性时发生错误。");
        }
    }

    // ConditionRow 属性变化时更新 UI
    private static void OnConditionRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        try
        {
            if (d is not RuleEditorConditionComponent component || e.NewValue is not ConditionRow row) return;
            component.UpdateUnitOptions(row.ConditionType); // 根据条件类型更新单位选项
            component.RaiseConditionChanged(); // 触发条件变化事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ConditionRow 属性变化时更新 UI 时发生错误。");
        }
    }

    // UI 控件值变化时更新 ConditionRow 的 ConditionType 属性
    private void OnConditionTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || ConditionTypeComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.ConditionType = selectedItem.Content.ToString(); // 更新条件类型
            UpdateUnitOptions(selectedItem.Content.ToString()); // 更新单位选项
            RaiseConditionChanged(); // 触发条件变化事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ConditionType 变化时发生错误。");
        }
    }

    // UI 控件值变化时更新 ConditionRow 的 Comparison 属性
    private void OnComparisonChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || ComparisonComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.Comparison = selectedItem.Content.ToString(); // 更新比较操作符
            RaiseConditionChanged(); // 触发条件变化事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Comparison 变化时发生错误。");
        }
    }

    // UI 控件值变化时更新 ConditionRow 的 Value 属性
    private void OnValueChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null) return;
            ConditionRow.Value = ValueTextBox.Text; // 更新条件值
            RaiseConditionChanged(); // 触发条件变化事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Value 变化时发生错误。");
        }
    }

    // UI 控件值变化时更新 ConditionRow 的 Unit 属性
    private void OnUnitChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || UnitComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.Unit = selectedItem.Content.ToString(); // 更新条件单位
            RaiseConditionChanged(); // 触发条件变化事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Unit 变化时发生错误。");
        }
    }

    // 更新单位选项可用性
    private void UpdateUnitOptions(string conditionType)
    {
        try
        {
            var isAge = conditionType == "年龄"; // 判断条件类型是否为“年龄”
            ConditionUnitOptions = isAge ? ["岁", "月"] : ["Kg"]; // 设置单位选项

            ConditionRow.Unit = isAge switch
            {
                true when ConditionRow.Unit == "Kg" => "岁",
                false => "Kg",
                _ => ConditionRow.Unit
            };
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "更新单位选项时发生错误。");
        }
    }

    // 通知条件变化
    private void RaiseConditionChanged()
    {
        try
        {
            ConditionChanged?.Invoke(this, EventArgs.Empty); // 触发 ConditionChanged 事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "触发 ConditionChanged 事件时发生错误。");
        }
    }

    // 触发删除事件
    private void OnDeleteCondition(object sender, RoutedEventArgs e)
    {
        try
        {
            ConditionDeleted?.Invoke(this, EventArgs.Empty); // 触发 ConditionDeleted 事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "触发 ConditionDeleted 事件时发生错误。");
        }
    }
}
