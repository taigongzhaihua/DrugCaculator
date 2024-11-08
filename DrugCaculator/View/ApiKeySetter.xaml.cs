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
using Setting = DrugCaculator.Properties.Settings;

namespace DrugCaculator.View
{
    public partial class ApiKeySetter : INotifyPropertyChanged
    {
        private string _apiKey;

        public string ApiKey
        {
            get => _apiKey;
            set => SetField(ref _apiKey, value, nameof(ApiKey));
        }

        public ApiKeySetter()
        {
            InitializeComponent();
            ApiKey = DeepSeekService.GetApiKeyFromSettings(); // 从设置中获取 API 密钥
        }

        // 当边框大小改变时，更新裁剪区域以匹配控件的当前大小和圆角
        private void RoundedBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Border border)
            {
                border.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                    RadiusX = border.CornerRadius.TopLeft,
                    RadiusY = border.CornerRadius.TopLeft
                };
            }
        }

        // 允许用户通过拖动窗口来移动位置
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // 当切换按钮选中时，显示密码
        private void ToggleVisibilityButton_Checked(object sender, RoutedEventArgs e)
        {
            SetPasswordVisibility(true);
        }

        // 当切换按钮取消选中时，隐藏密码
        private void ToggleVisibilityButton_Unchecked(object sender, RoutedEventArgs e)
        {
            SetPasswordVisibility(false);
        }

        // 设置密码框的可见性
        private void SetPasswordVisibility(bool isVisible)
        {
            if (isVisible)
            {
                TextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                TextBox.Visibility = Visibility.Visible;
                ToggleVisibilityButton.Content = CreatePathGeometry("M1 8 A 10 10 0 0 1 19 8 M 10 10 m -3.5 0 a 3.5 3.5 0 1 0 7 0 a 3.5 3.5 0 1 0 -7 0", Brushes.RoyalBlue);
            }
            else
            {
                PasswordBox.Password = TextBox.Text;
                TextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                ToggleVisibilityButton.Content = EyeClosedIcon;
            }
        }

        // 创建路径几何，用于切换按钮的图标
        private static Path CreatePathGeometry(string data, Brush stroke)
        {
            return new Path
            {
                Data = Geometry.Parse(data),
                Stroke = stroke,
                StrokeThickness = 1.7
            };
        }

        // 确认按钮点击事件处理程序
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var apiKey = PasswordBox.Visibility == Visibility.Visible ? PasswordBox.Password : TextBox.Text;

            // 检查 API 密钥是否为空
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine(@"API 密钥为空");
                MessageBox.Show("API 密钥不能为空。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // 加密 API 密钥并保存到设置中
                Console.WriteLine(@"开始加密 API 密钥");
                var encryptedApiKey = EncryptionService.Encrypt(apiKey, "DeepSeekApiKey");
                Setting.Default.DeepSeekApiKey = encryptedApiKey;
                Setting.Default.Save();
                Console.WriteLine(@"API 密钥已成功保存");
                MessageBox.Show("API 密钥已成功保存。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"保存 API 密钥时发生错误: {ex.Message}");
                MessageBox.Show($"保存 API 密钥时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 取消按钮点击事件处理程序
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 属性改变时通知界面更新
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 设置属性值并通知界面更新
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}