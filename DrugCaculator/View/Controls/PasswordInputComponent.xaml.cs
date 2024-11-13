using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DrugCalculator.View.Controls
{
    public partial class PasswordInputComponent
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                nameof(Password),
                typeof(string),
                typeof(PasswordInputComponent),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordInputComponent component)
            {
                component.PasswordBoxX.Password = (string)e.NewValue;
                component.Password = (string)e.NewValue;
            }
        }

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public PasswordInputComponent()
        {
            InitializeComponent();
            PasswordBoxX.PasswordChanged += PasswordBoxX_PasswordChanged;
        }

        // 当密码框内容更改时，更新 ApiKey 属性
        private void PasswordBoxX_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // 更新 ApiKey 以保持同步
            if (PasswordBoxX.Password != Password)
            {
                Password = PasswordBoxX.Password;
            }
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
                TextBox.Visibility = Visibility.Visible;
                PasswordBoxX.Visibility = Visibility.Collapsed;
                TextBox.Text = Password;
                ToggleVisibilityButton.Content = CreatePathGeometry("M1 8 A 10 10 0 0 1 19 8 M 10 10 m -3.5 0 a 3.5 3.5 0 1 0 7 0 a 3.5 3.5 0 1 0 -7 0", Brushes.RoyalBlue);
            }
            else
            {
                PasswordBoxX.Visibility = Visibility.Visible;
                TextBox.Visibility = Visibility.Collapsed;
                PasswordBoxX.Password = Password;
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
    }
}
