using DrugCalculator.Properties;
using DrugCalculator.View.Components;
using DrugCalculator.ViewModels;
using NLog;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

// ReSharper disable UnusedMember.Global

namespace DrugCalculator.View.Windows;

public partial class MainWindow : CustomDialog
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public MainWindow()
    {
        InitializeComponent();
        Logger.Info("初始化 MainWindow 窗口");
        InitializeWindowSettings(); // 初始化窗口设置
        Closing += MainWindow_Closing; // 订阅窗口关闭事件
        SubscribeViewModelEvents(); // 订阅 ViewModel 的 PropertyChanged 事件
    }

    // 初始化窗口设置
    private void InitializeWindowSettings()
    {
        Logger.Info("初始化窗口设置");
        // 设置窗口在屏幕中央显示
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    // 订阅 ViewModel 的 PropertyChanged 事件
    private void SubscribeViewModelEvents()
    {
        Logger.Info("订阅 ViewModel 的 PropertyChanged 事件");
        // 订阅 ViewModel 的 PropertyChanged 事件，以便在属性变化时更新 UI
        if (DataContext is MainWindowViewModel viewModel) viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    // 窗口关闭事件处理
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        Logger.Info("处理窗口关闭事件");
        // 检查是否已经有保存的用户选择
        var savedChoice = Settings.Default.IsClose;

        if (savedChoice is not ("Ask" or null or ""))
        {
            Logger.Info("找到已保存的用户选择: " + savedChoice);
            HandleUserChoice(savedChoice, e);
            return;
        }

        // 弹出自定义对话框获取用户选择
        Logger.Info("弹出 CloseConfirmationDialog 获取用户选择");
        var dialog = new CloseConfirmationDialog { Owner = this };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            if (dialog.RememberChoice)
            {
                // 保存用户选择到设置中
                Settings.Default.IsClose = dialog.IsClose ? "Close" : "Minimize";
                Settings.Default.Save();
                Logger.Info("保存用户选择: " + Settings.Default.IsClose);
            }

            HandleUserChoice(dialog.IsClose ? "Close" : "Minimize", e);
        }
        else
        {
            // 用户取消关闭
            Logger.Info("用户取消关闭窗口");
            e.Cancel = true;
        }
    }

    // 处理用户选择
    private void HandleUserChoice(string choice, CancelEventArgs e)
    {
        Logger.Info("处理用户选择: " + choice);
        switch (choice)
        {
            case "Close":
                // 用户选择了关闭程序
                Logger.Info("用户选择关闭程序");
                ExitApplication();
                break;
            case "Minimize":
                // 用户选择了最小化到托盘
                Logger.Info("用户选择最小化到托盘");
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                Hide();
                break;
        }
    }

    // 退出应用程序
    private static void ExitApplication()
    {
        Logger.Info("退出应用程序");
        // _notifyIcon.Dispose(); // 释放托盘图标
        Application.Current.Shutdown(); // 退出应用程序
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击关闭按钮");
        Close();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击最小化按钮");
        WindowState = WindowState.Minimized;
    }

    protected override void OnClosed(EventArgs e)
    {
        Logger.Info("窗口已关闭，注销全局快捷键");
        base.OnClosed(e);
    }

    private void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { ContextMenu: not null } button) return;
        // 手动设置 ContextMenu 的位置
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;

        // 显示或隐藏 ContextMenu
        button.ContextMenu.IsOpen = !button.ContextMenu.IsOpen;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "SelectedDrug") return;
        Logger.Info("ViewModel 属性改变: " + e.PropertyName);
        // 当 SelectedDrug 改变时，让 WeightTextBox 获得焦点
        if (SearchTextBox.IsFocused) return;
        WeightTextBox.Focus();
        if (WeightTextBox.Text == "0") WeightTextBox.Text = "";
    }

    private void WeightTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Tab or Key.Space)) return;
        Logger.Info("WeightTextBox 键盘事件: " + e.Key);
        // 当按下回车键时，将焦点设置到 AgeTextBox
        AgeTextBox.Focus();
        if (AgeTextBox.Text == "0") AgeTextBox.Text = "";
        if (WeightTextBox.Text == "") WeightTextBox.Text = "0";
    }

    private void AgeTextBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Tab or Key.Space)) return;
        Logger.Info("AgeTextBox 键盘事件: " + e.Key);
        // 当按下回车键时，将焦点设置到 AgeUnitComboBox
        AgeUnitComboBox.IsDropDownOpen = true;
        AgeUnitComboBox.Focus();
        if (AgeTextBox.Text == "") AgeTextBox.Text = "0";
    }

    private void AgeUnitComboBox_KeyDown(object sender, KeyEventArgs e)
    {
        Logger.Info("AgeUnitComboBox 键盘事件: " + e.Key);
        switch (e.Key)
        {
            case Key.Enter:
                if (AgeUnitComboBox.IsDropDownOpen == false)
                    AgeUnitComboBox.IsDropDownOpen = !AgeUnitComboBox.IsDropDownOpen;
                break;
            case Key.Down or Key.Space:
                AgeUnitComboBox.IsDropDownOpen = true;
                break;
            case Key.Tab:
                {
                    if (WeightTextBox.Text == "0") WeightTextBox.Text = "";
                    WeightTextBox.Focus();
                    break;
                }
        }
    }

    private void AgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Logger.Info("AgeTextBox 预览文本输入事件: " + e.Text);
        // 允许数字和一个小数点
        var regex = AgeTextRegex();
        e.Handled = !regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text));
    }

    private void WeightTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Logger.Info("WeightTextBox 预览文本输入事件: " + e.Text);
        // 允许数字和一个小数点
        var regex = WeightTextRegex();
        e.Handled = !regex.IsMatch(((TextBox)sender).Text.Insert(((TextBox)sender).SelectionStart, e.Text));
    }

    private void SearchTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        Logger.Info("SearchTextBox 加载事件");
        InputLanguageManager.SetInputLanguage((TextBox)sender, new CultureInfo("en-US"));
    }

    [GeneratedRegex(@"^[0-9]*")]
    private static partial Regex AgeTextRegex();
    [GeneratedRegex(@"^[0-9]*(?:\.[0-9]*)?$")]
    private static partial Regex WeightTextRegex();
}