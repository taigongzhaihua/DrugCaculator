using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrugCaculator.View
{
    public partial class CloseConfirmationDialog : Window
    {
        public bool IsClose { get; private set; }
        public bool RememberChoice { get; private set; }

        public CloseConfirmationDialog()
        {
            InitializeComponent();
            RbClose.IsChecked = true; // 默认选择关闭程序
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IsClose = RbClose.IsChecked == true;
            RememberChoice = CbRememberChoice.IsChecked == true;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
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
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}