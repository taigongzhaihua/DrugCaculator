using DrugCalculator.Models;
using DrugCalculator.Services;
using DrugCalculator.View.Components;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DrugCalculator.View.Controls;

public partial class RuleEditorConditionComponent : INotifyPropertyChanged
{
    // Logger实例用于记录错误和信息，使用NLog库来帮助调试和监控应用程序的行为。
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // ConfigurationService实例用于管理配置选项
    // 设计原理：采用单例模式来确保配置服务的唯一性，从而保证所有组件对配置选项的访问一致性。
    private readonly ConfigurationService _configurationService = ConfigurationService.Instance; // 单例模式以确保一致的配置访问

    // 通知删除条件的事件
    // 设计原理：事件机制用于解耦，允许外部订阅者知道组件内部的删除行为，以便在删除条件时执行特定操作。
    public event EventHandler ConditionDeleted; // 当删除按钮被点击时触发

    // 通知条件更改的事件
    // 设计原理：使用事件通知外部，当组件内部的数据变化时，外部组件或服务可以响应这些变化，保持状态同步。
    public event EventHandler ConditionChanged; // 当任何条件属性更改时触发

    // 构造函数初始化组件并加载所需的选项
    // 设计原理：在组件初始化时，加载所有必要的选项数据，确保界面在显示时具有完整的信息。
    public RuleEditorConditionComponent()
    {
        try
        {
            InitializeComponent();
            LoadOptions(); // 加载条件、单位和比较选项
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "初始化 RuleEditorConditionComponent 时发生错误。");
            CustomMessageBox.Show("初始化组件时发生错误，请检查日志以获取详细信息。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    // 用于存储条件类型选项的ObservableCollection
    // UI数据绑定的属性，带有更改通知
    // 设计原理：使用ObservableCollection以支持动态数据更新和UI的自动刷新。
    private ObservableCollection<string> _conditionTypeOptions;
    public ObservableCollection<string> ConditionTypeOptions
    {
        get => _conditionTypeOptions;
        set
        {
            try
            {
                _conditionTypeOptions = value;
                OnPropertyChanged(nameof(ConditionTypeOptions)); // 通知UI属性更改
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置 ConditionTypeOptions 属性时发生错误。");
            }
        }
    }

    // 用于存储条件单位选项的ObservableCollection
    // 该集合可以根据条件类型选择动态更新
    // 设计原理：允许用户根据选择的条件类型更改单位选项，实现动态UI响应，提升用户体验。
    private ObservableCollection<string> _conditionUnitOptions;
    public ObservableCollection<string> ConditionUnitOptions
    {
        get => _conditionUnitOptions;
        set
        {
            try
            {
                _conditionUnitOptions = value;
                OnPropertyChanged(nameof(ConditionUnitOptions)); // 通知UI属性更改
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置 ConditionUnitOptions 属性时发生错误。");
            }
        }
    }

    // 用于存储比较选项的ObservableCollection
    // 设计原理：将比较操作符作为动态绑定的集合，以便在不同场景下提供合适的比较选项。
    private ObservableCollection<string> _comparisonOptions;
    public ObservableCollection<string> ComparisonOptions
    {
        get => _comparisonOptions;
        set
        {
            try
            {
                _comparisonOptions = value;
                OnPropertyChanged(nameof(ComparisonOptions)); // 通知UI属性更改
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置 ComparisonOptions 属性时发生错误。");
            }
        }
    }

    // 控制删除按钮可见性的DependencyProperty
    // 需要时隐藏删除按钮
    // 设计原理：使用依赖属性以支持XAML中的数据绑定和样式设置，从而根据不同条件动态控制UI元素的可见性。
    public static readonly DependencyProperty IsDeleteVisibleProperty =
        DependencyProperty.Register(
            nameof(IsDeleteVisible),
            typeof(bool),
            typeof(RuleEditorConditionComponent),
            new PropertyMetadata(true, OnIsDeleteVisibleChanged));

    // 获取/设置删除按钮可见状态的属性
    public bool IsDeleteVisible
    {
        get => (bool)GetValue(IsDeleteVisibleProperty);
        set
        {
            try
            {
                SetValue(IsDeleteVisibleProperty, value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置 IsDeleteVisible 属性时发生错误。");
            }
        }
    }

    // 绑定ConditionRow数据到UI的DependencyProperty
    // 允许直接从UI编辑条件行数据
    // 设计原理：使用双向绑定以确保UI和数据模型之间的同步更新，减少手动同步代码，提高可维护性。
    public static readonly DependencyProperty ConditionRowProperty =
        DependencyProperty.Register(
            nameof(ConditionRow),
            typeof(ConditionRow),
            typeof(RuleEditorConditionComponent),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, // 支持UI和数据模型之间的双向绑定
                OnConditionRowChanged));

    // 获取/设置ConditionRow对象的属性，启用UI绑定特定条件数据
    public ConditionRow ConditionRow
    {
        get => (ConditionRow)GetValue(ConditionRowProperty);
        set
        {
            try
            {
                SetValue(ConditionRowProperty, value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "设置 ConditionRow 属性时发生错误。");
            }
        }
    }

    // 实现INotifyPropertyChanged接口
    // 用于在属性值更改时通知UI
    // 设计原理：通过实现INotifyPropertyChanged接口来支持数据绑定，使得UI能够在数据发生变化时自动更新，提升交互性和响应速度。
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        try
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); // 通知属性更改
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"在属性 {propertyName} 更改时通知UI时发生错误。");
        }
    }

    // 从配置服务加载条件类型、单位和比较选项
    // 设计原理：集中式配置管理，使选项加载逻辑统一且可重用，方便后续修改和扩展。
    private void LoadOptions()
    {
        try
        {
            // 使用ConfigurationService获取并初始化下拉框的选项
            var conditionTypeList = _configurationService.GetOption("ConditionTypeOptions");
            ConditionTypeOptions = new ObservableCollection<string>(conditionTypeList);

            var conditionUnitList = _configurationService.GetOption("ConditionUnitOptions");
            ConditionUnitOptions = new ObservableCollection<string>(conditionUnitList);

            var comparisonList = _configurationService.GetOption("ComparisonOptions");
            ComparisonOptions = new ObservableCollection<string>(comparisonList);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "加载选项时发生错误。");
            CustomMessageBox.Show("加载选项时发生错误，请检查日志以获取详细信息。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    // 删除按钮可见性更改时的回调
    // 更新UI中删除按钮的可见性
    // 设计原理：使用依赖属性的更改回调，以确保在属性值更改时能够动态响应并调整UI元素的状态。
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
            Logger.Error(ex, "更新删除按钮可见性时发生错误。");
        }
    }

    // ConditionRow属性更改时的回调
    // 更新单位选项并触发ConditionChanged事件
    // 设计原理：保持UI与数据模型同步，当ConditionRow发生变化时，自动更新其他相关UI部分，确保数据一致性。
    private static void OnConditionRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        try
        {
            if (d is not RuleEditorConditionComponent component || e.NewValue is not ConditionRow _) return;
            component.UpdateUnitOptions(); // 根据选择的条件类型更新单位选项
            component.RaiseConditionChanged(); // 触发ConditionChanged事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "更新UI以响应ConditionRow属性更改时发生错误。");
        }
    }

    // 条件类型在UI中更改时的事件处理程序
    // 更新ConditionRow的ConditionType属性并更新可用的单位选项
    // 设计原理：确保用户选择的条件类型变化时，相关的单位选项也会自动调整，避免用户选择无效的组合。
    private void OnConditionTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || ConditionTypeComboBox.SelectedItem is not string selectedItem) return;
            ConditionRow.ConditionType = selectedItem; // 更新ConditionRow模型
            UpdateUnitOptions(); // 根据选择的条件类型更新单位选项
            RaiseConditionChanged(); // 通知更改
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "条件类型更改时发生错误。");
        }
    }

    // 比较操作符更改时的事件处理程序
    // 更新ConditionRow中的Comparison属性
    // 设计原理：通过更新数据模型来保持用户界面和数据的一致性，确保用户输入的每个变化都反映在底层数据中。
    private void OnComparisonChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || ComparisonComboBox.SelectedItem is not string selectedItem) return;
            ConditionRow.Comparison = selectedItem; // 更新Comparison属性
            RaiseConditionChanged(); // 通知更改
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "比较操作符更改时发生错误。");
        }
    }

    // 值在UI中更改时的事件处理程序
    // 更新ConditionRow中的Value属性
    // 设计原理：通过监听文本框内容的变化来实时更新数据模型，确保数据和用户输入保持同步。
    private void OnValueChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null) return;
            ConditionRow.Value = ValueTextBox.Text; // 更新Value属性
            RaiseConditionChanged(); // 通知更改
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "值更改时发生错误。");
        }
    }

    // 单位在UI中更改时的事件处理程序
    // 更新ConditionRow中的Unit属性
    // 设计原理：通过更新ConditionRow的Unit属性，保持界面和数据模型之间的同步，确保用户选择的每个单位都得到正确处理。
    private void OnUnitChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ConditionRow == null || UnitComboBox.SelectedItem is not string selectedItem) return;
            ConditionRow.Unit = selectedItem; // 更新Unit属性
            RaiseConditionChanged(); // 通知更改
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "单位更改时发生错误。");
        }
    }

    // 根据选择的条件类型更新单位选项
    // 例如，如果选择了“年龄”，单位将是“岁”或“月”
    // 设计原理：根据不同的条件类型提供适当的单位选择，确保输入的有效性和一致性。
    private void UpdateUnitOptions()
    {
        try
        {
            var isAge = ConditionRow.ConditionType == "年龄"; // 检查条件类型是否为“年龄”
            if (isAge)
            {
                ConditionUnitOptions = ["岁", "月"]; // 年龄的单位为“岁”或“月”
                UnitComboBox.SelectedItem = string.IsNullOrEmpty(ConditionRow.Unit) ? "岁" : ConditionRow.Unit; // 如果没有设置单位，默认为“岁”
                UnitComboBox.IsEnabled = true; // 年龄需要单位，启用单位选择
            }
            else
            {
                ConditionUnitOptions = ["Kg"]; // 其他条件类型默认为“Kg”
                UnitComboBox.SelectedItem = "Kg";
                UnitComboBox.IsEnabled = false; // 其他条件类型不需要单位，禁用单位选择
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "更新单位选项时发生错误。");
        }
    }

    // 触发ConditionChanged事件的方法
    // 通知任何订阅者条件属性已更改
    // 设计原理：通过事件机制实现组件与外部之间的解耦，让外部组件可以响应条件变化。
    private void RaiseConditionChanged()
    {
        try
        {
            ConditionChanged?.Invoke(this, EventArgs.Empty); // 触发事件
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "触发ConditionChanged事件时发生错误。");
        }
    }

    // 删除条件按钮点击时的事件处理程序
    // 触发ConditionDeleted事件
    // 设计原理：通过事件通知外部组件条件已被删除，允许外部进行相应的处理，例如更新UI或数据源。
    private void OnDeleteCondition(object sender, RoutedEventArgs e)
    {
        try
        {
            ConditionDeleted?.Invoke(this, EventArgs.Empty); // 触发事件通知条件已删除
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "触发ConditionDeleted事件时发生错误。");
        }
    }
}
