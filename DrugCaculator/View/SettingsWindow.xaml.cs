using System.Windows;
using System.Windows.Controls;

namespace DrugCaculator.View
{
    /// <summary>
    /// SettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            // 窗口加载时设置单选框状态
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            var isCloseSetting = Properties.Settings.Default.IsClose;

            switch (isCloseSetting)
            {
                case "Close":
                    ExitProgramRadioButton.IsChecked = true;
                    break;
                case "Minimize":
                    MinimizeToTrayRadioButton.IsChecked = true;
                    break;
                default:
                    AskEveryTimeRadioButton.IsChecked = true;
                    break;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var apiKeySetter = new ApiKeySetter()
            {
                Owner = Window.GetWindow((sender as Button)!)
            };
            apiKeySetter.ShowDialog();
        }
    }
}