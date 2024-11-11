using System.Windows;

namespace DrugCaculator.View.Windows;

public partial class CloseConfirmationDialog
{
    // 是否关闭程序的标志
    public bool IsClose { get; private set; }
    // 是否记住用户选择的标志
    public bool RememberChoice { get; private set; }

    public CloseConfirmationDialog()
    {
        InitializeComponent();
        InitializeDefaultSettings();
        DialogHeight = 200;
        // DialogWidth = 400;
    }

    // 初始化默认设置
    private void InitializeDefaultSettings()
    {
        RbClose.IsChecked = true; // 默认选择关闭程序
    }

    // "确定" 按钮点击事件处理
    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        SetDialogResults(); // 设置对话框结果
        DialogResult = true; // 设置对话框结果为 true，表示用户确认
    }

    // "取消" 按钮点击事件处理
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false; // 设置对话框结果为 false，表示用户取消
    }

    // 设置对话框的结果，根据用户的选择更新 IsClose 和 RememberChoice 属性
    private void SetDialogResults()
    {
        IsClose = RbClose.IsChecked == true;
        RememberChoice = CbRememberChoice.IsChecked == true;
    }
}