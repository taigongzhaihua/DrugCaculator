using DrugCaculator.Properties;
using DrugCaculator.Utilities.Helpers;
using DrugCaculator.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;
// ReSharper disable UnusedMember.Global

namespace DrugCaculator.View.Windows;


public partial class MainWindow
{
    private NotifyIcon _notifyIcon;
    private const int HotkeyId = 9000; // 热键ID
    private const int ModWin = 0x0008; // Win 键修饰符
    private const int VkF2 = 0x71; // F2 键的虚拟码

    public MainWindow()
    {
        InitializeComponent();
        InitializeWindowSettings(); // 初始化窗口设置
        RegisterHotKeys(); // 注册全局快捷键
        InitializeNotifyIcon(); // 初始化托盘图标
        InitializeContextMenu(); // 初始化右键菜单
        Closing += MainWindow_Closing; // 订阅窗口关闭事件
        SubscribeViewModelEvents(); // 订阅 ViewModel 的 PropertyChanged 事件
    }

    // 初始化窗口设置
    private void InitializeWindowSettings()
    {
        // 设置窗口在屏幕中央显示
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    // 初始化托盘图标
    private void InitializeNotifyIcon()
    {
        // 初始化托盘图标并设置属性
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon(fileName: "AppIcon.ico"), // 使用指定的图标文件
            Visible = true,
            Text = @"药物查询"
        };
        // 双击托盘图标时显示窗口
        _notifyIcon.DoubleClick += (_, _) => ShowWindow();
    }

    // 初始化右键菜单
    private void InitializeContextMenu()
    {
        // 初始化托盘图标的右键菜单
        var contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Items.Add("打开", null, (_, _) => ShowWindow()); // 显示窗口选项
        contextMenuStrip.Items.Add("退出", null, (_, _) => ExitApplication()); // 退出应用程序选项
        _notifyIcon.ContextMenuStrip = contextMenuStrip;
    }

    // 订阅 ViewModel 的 PropertyChanged 事件
    private void SubscribeViewModelEvents()
    {
        // 订阅 ViewModel 的 PropertyChanged 事件，以便在属性变化时更新 UI
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    // 注册全局快捷键
    private void RegisterHotKeys()
    {
        var helper = new WindowInteropHelper(this);
        HotKeyHelper.Register(helper.Handle, HotkeyId, ModWin, VkF2);
        ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
    }

    // 处理热键消息
    private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
        if (msg.message != 0x0312 || msg.wParam.ToInt32() != HotkeyId) return; // 0x0312 为 WM_HOTKEY
        ShowWindow(); // 显示窗口
        handled = true;
    }

    // 窗口关闭事件处理
    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        // 检查是否已经有保存的用户选择
        var savedChoice = Settings.Default.IsClose;


        if (savedChoice is not "Ask" and not null)
        {
            HandleUserChoice(savedChoice, e);
            return;
        }

        // 弹出自定义对话框获取用户选择
        var dialog = new CloseConfirmationDialog { Owner = this };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            if (dialog.RememberChoice)
            {
                // 保存用户选择到设置中
                Settings.Default.IsClose = dialog.IsClose ? "Close" : "Minimize";
                Settings.Default.Save();
            }
            HandleUserChoice(dialog.IsClose ? "Close" : "Minimize", e);
        }
        else
        {
            // 用户取消关闭
            e.Cancel = true;
        }
    }
    // 处理用户选择
    private void HandleUserChoice(string choice, CancelEventArgs e)
    {
        switch (choice)
        {
            case "Close":
                // 用户选择了关闭程序
                ExitApplication();
                break;
            case "Minimize":
                // 用户选择了最小化到托盘
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                Hide();
                break;
        }
    }

    // 退出应用程序
    private void ExitApplication()
    {
        _notifyIcon.Dispose(); // 释放托盘图标
        Application.Current.Shutdown(); // 退出应用程序
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
    protected override void OnClosed(EventArgs e)
    {
        // 注销热键
        var helper = new WindowInteropHelper(this);
        HotKeyHelper.Unregister(helper.Handle, HotkeyId);
        base.OnClosed(e);
    }

    private void ShowWindow()
    {
        Show(); // 显示窗口
        WindowState = WindowState.Normal; // 设置窗口状态
        Activate(); // 激活窗口
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "SelectedDrug") return;
        // 当 SelectedDrug 改变时，让 WeightTextBox 获得焦点
        if (SearchTextBox.IsFocused) return;
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
    private void SearchTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        InputLanguageManager.SetInputLanguage((TextBox)sender, new CultureInfo("en-US"));
    }
}