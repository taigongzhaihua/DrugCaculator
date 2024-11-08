using System;
using System.Windows;
using System.Windows.Controls;

namespace DrugCaculator.View.Components
{
    public partial class RuleEditorConditionComponent
    {
        // 删除事件
        public event EventHandler ConditionDeleted;
        public event EventHandler ConditionChanged;

        public RuleEditorConditionComponent()
        {
            InitializeComponent();
        }

        // 定义 IsDeleteVisible 依赖属性
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

        // 定义 ConditionRow 依赖属性
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

        // 更新删除按钮的可见性
        private static void OnIsDeleteVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RuleEditorConditionComponent component)
            {
                component.DeleteButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // ConditionRow 属性变化时更新 UI
        private static void OnConditionRowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RuleEditorConditionComponent component && e.NewValue is ConditionRow row)
            {
                component.UpdateUnitOptions(row.ConditionType);
                component.RaiseConditionChanged();
            }
        }

        // UI 控件值变化时更新 ConditionRow
        private void OnConditionTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConditionRow == null || ConditionTypeComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.ConditionType = selectedItem.Content.ToString();
            UpdateUnitOptions(selectedItem.Content.ToString());
            RaiseConditionChanged();
            Console.WriteLine(ConditionRow.ToString());
        }

        private void OnComparisonChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConditionRow == null || ComparisonComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.Comparison = selectedItem.Content.ToString();
            RaiseConditionChanged();
            Console.WriteLine(ConditionRow.ToString());
        }

        private void OnValueChanged(object sender, TextChangedEventArgs e)
        {
            if (ConditionRow == null) return;
            ConditionRow.Value = ValueTextBox.Text;
            RaiseConditionChanged();
            Console.WriteLine(ConditionRow.ToString());
        }

        private void OnUnitChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConditionRow == null || UnitComboBox.SelectedItem is not ComboBoxItem selectedItem) return;
            ConditionRow.Unit = selectedItem.Content.ToString();
            RaiseConditionChanged();
            Console.WriteLine(ConditionRow.ToString());
        }

        // 更新单位选项可用性
        private void UpdateUnitOptions(string conditionType)
        {
            bool isAge = conditionType == "年龄";
            BoxItemKg.IsEnabled = !isAge;
            BoxItemKg.Visibility = isAge ? Visibility.Collapsed : Visibility.Visible;

            BoxItemMonth.IsEnabled = isAge;
            BoxItemMonth.Visibility = isAge ? Visibility.Visible : Visibility.Collapsed;

            BoxItemYears.IsEnabled = isAge;
            BoxItemYears.Visibility = isAge ? Visibility.Visible : Visibility.Collapsed;
            if (isAge && ConditionRow.Unit == "Kg")
            {
                BoxItemYears.IsSelected = true;
            }
            else if (!isAge && ConditionRow.Unit != "Kg")
            {
                BoxItemKg.IsSelected = true;
            }
        }

        // 通知条件变化
        private void RaiseConditionChanged()
        {
            ConditionChanged?.Invoke(this, EventArgs.Empty);
        }

        // 触发删除事件
        private void OnDeleteCondition(object sender, EventArgs e)
        {
            ConditionDeleted?.Invoke(this, EventArgs.Empty);
        }
    }
}
