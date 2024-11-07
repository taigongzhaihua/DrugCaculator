using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrugCaculator.View
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            // 窗口加载时设置单选框状态
            LoadUserSettings();
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void LoadUserSettings()
        {
            string isCloseSetting = Properties.Settings.Default.IsClose;

            if (isCloseSetting == "Close")
            {
                ExitProgramRadioButton.IsChecked = true;
            }
            else if (isCloseSetting == "Minimize")
            {
                MinimizeToTrayRadioButton.IsChecked = true;
            }
            else
            {
                AskEveryTimeRadioButton.IsChecked = true;
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ExitProgramRadioButton.IsChecked == true)
            {
                Properties.Settings.Default.IsClose = "Close";
            }
            else if (MinimizeToTrayRadioButton.IsChecked == true)
            {
                Properties.Settings.Default.IsClose = "Minimize";
            }
            else if (AskEveryTimeRadioButton.IsChecked == true)
            {
                Properties.Settings.Default.IsClose = null;
            }

            // 保存设置以确保下次启动时加载
            Properties.Settings.Default.Save();
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var apiKeySetter = new ApiKeySetter();
            apiKeySetter.ShowDialog();
        }
    }
}
