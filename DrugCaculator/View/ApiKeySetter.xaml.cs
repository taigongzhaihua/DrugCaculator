using DrugCaculator.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrugCaculator.View
{
    public partial class ApiKeySetter : INotifyPropertyChanged
    {
        private string _apiKey;

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                OnPropertyChanged();
                PasswordBox.Password = value;
            }
        }
        public ApiKeySetter()
        {
            InitializeComponent();
            ApiKey = DeepSeekService.GetApiKeyFromSettings();

        }
        private void RoundedBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not Border border) return;

            // 更新裁剪区域，以匹配控件的当前大小和圆角
            border.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ToggleVisibilityButton_Checked(object sender, RoutedEventArgs e)
        {
            TextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            TextBox.Visibility = Visibility.Visible;
            ToggleVisibilityButton.Content = new Path
            {
                Data = Geometry.Parse("M1 8 A 10 10 0 0 1 19 8 M 10 10 m -3.5 0 a 3.5 3.5 0 1 0 7 0 a 3.5 3.5 0 1 0 -7 0"),
                Stroke = Brushes.RoyalBlue,
                StrokeThickness = 1.7
            };
        }

        private void ToggleVisibilityButton_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = TextBox.Text;
            TextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            ToggleVisibilityButton.Content = EyeClosedIcon;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取输入的 API 密钥
            var apiKey = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : TextBox.Text;

            // 打印日志：确认按钮被点击
            Console.WriteLine(@"确认按钮被点击");

            // 检查输入
            if (string.IsNullOrWhiteSpace(apiKey)) // 检查 API 密钥是否为空或仅包含空白字符
            {
                // 打印日志：API 密钥为空
                Console.WriteLine(@"API 密钥为空");
                MessageBox.Show("API 密钥不能为空。", "错误", MessageBoxButton.OK, MessageBoxImage.Error); // 显示错误消息框
                return; // 退出事件处理程序
            }

            // 加密并存储到应用程序设置
            try
            {
                // 打印日志：开始加密 API 密钥
                Console.WriteLine(@"开始加密 API 密钥");
                var encryptedApiKey = EncryptionService.Encrypt(apiKey, "DeepSeekApiKey"); // 使用加密服务加密 API 密钥
                Properties.Settings.Default.DeepSeekApiKey = encryptedApiKey; // 将加密后的 API 密钥存储到应用程序设置中
                Properties.Settings.Default.Save(); // 保存设置
                // 打印日志：API 密钥已成功保存
                Console.WriteLine(@"API 密钥已成功保存");
                MessageBox.Show("API 密钥已成功保存。", "成功", MessageBoxButton.OK, MessageBoxImage.Information); // 显示成功消息框
                Close(); // 关闭输入窗口
            }
            catch (Exception ex)
            {
                // 打印日志：保存 API 密钥时发生错误
                Console.WriteLine($@"保存 API 密钥时发生错误: {ex.Message}");
                MessageBox.Show($"保存 API 密钥时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); // 显示错误消息框
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}