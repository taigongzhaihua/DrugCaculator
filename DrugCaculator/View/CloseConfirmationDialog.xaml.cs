using System.Windows;

namespace DrugCaculator.View
{
    public partial class CloseConfirmationDialog : Window
    {
        public bool IsClose { get; private set; }
        public bool RememberChoice { get; private set; }

        public CloseConfirmationDialog()
        {
            InitializeComponent();
            rbClose.IsChecked = true; // 默认选择关闭程序
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            IsClose = rbClose.IsChecked == true;
            RememberChoice = cbRememberChoice.IsChecked == true;
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}