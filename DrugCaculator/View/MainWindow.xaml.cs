using DrugCaculator.Properties;
using DrugCaculator.ViewModels;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

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
                return totalHeight - 165; // 可以根据需要调整减去的值
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
            if (Regex.IsMatch(value?.ToString()!, @"\D"))
            {
                value = Regex.Replace(value?.ToString()!, @"\D", "");
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
        private readonly NotifyIcon _notifyIcon;
        private const int HotkeyId = 9000; // 热键ID
        private const int ModWin = 0x0008; // Win 键修饰符
        private const int VkF2 = 0x71; // F2 键的虚拟码

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public MainWindow()
        {
            InitializeComponent();
            // 注册全局快捷键 Win + F2
            var helper = new WindowInteropHelper(this);
            RegisterHotKey(helper.Handle, HotkeyId, ModWin, VkF2);

            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
            // 初始化托盘图标
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(fileName: "AppIcon.ico"), // 使用你的图标文件
                Visible = true,
                Text = @"药物查询"
            };
            _notifyIcon.DoubleClick += (_, _) => ShowWindow();

            // 处理窗口最小化事件
            StateChanged += MainWindow_StateChanged;

            // 初始化右键菜单
            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("显示", null, (_, _) => ShowWindow());
            contextMenuStrip.Items.Add("退出", null, (_, _) => ExitApplication());

            // 将右键菜单赋给托盘图标
            _notifyIcon.ContextMenuStrip = contextMenuStrip;

            // 处理关闭事件
            Closing += MainWindow_Closing;
            // 订阅 ViewModel 的 PropertyChanged 事件
            if (DataContext is DrugViewModel viewModel)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {

            }
        }
        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == 0x0312 && msg.wParam.ToInt32() == HotkeyId) // 0x0312 为 WM_HOTKEY
            {
                ShowWindow(); // 显示窗口
                handled = true;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            // 检查是否已经有保存的用户选择
            string savedChoice = Settings.Default.IsClose;
            if (!string.IsNullOrEmpty(savedChoice))
            {
                if (savedChoice == "Close")
                {
                    // 用户选择了关闭程序
                    _notifyIcon.Dispose();
                    return; // 继续关闭窗口
                }

                if (savedChoice == "Minimize")
                {
                    // 用户选择了最小化到托盘
                    e.Cancel = true;
                    WindowState = WindowState.Minimized;
                    Hide();
                    return;
                }
            }

            // 弹出自定义对话框
            var dialog = new CloseConfirmationDialog
            {
                Owner = this
            };

            var result = dialog.ShowDialog();
            if (result == true)
            {
                if (dialog.RememberChoice)
                {
                    // 保存用户选择到设置中
                    Settings.Default.IsClose = dialog.IsClose ? "Close" : "Minimize";
                    Settings.Default.Save();
                }

                if (dialog.IsClose)
                {
                    // 用户选择关闭程序
                    _notifyIcon.Dispose();
                }
                else
                {
                    // 用户选择最小化到托盘
                    e.Cancel = true;
                    WindowState = WindowState.Minimized;
                    Hide();
                }
            }
            else
            {
                // 用户取消关闭
                e.Cancel = true;
            }
        }
        private void ExitApplication()
        {
            _notifyIcon.Dispose(); // 释放托盘图标
            Application.Current.Shutdown(); // 退出应用程序
        }
        protected override void OnClosed(EventArgs e)
        {
            // 注销热键
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HotkeyId);
            base.OnClosed(e);
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
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
}
