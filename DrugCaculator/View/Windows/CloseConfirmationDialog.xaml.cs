using NLog;
using System.Windows;

namespace DrugCaculator.View.Windows;

public partial class CloseConfirmationDialog
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // 是否关闭程序的标志
    public bool IsClose { get; private set; }
    // 是否记住用户选择的标志
    public bool RememberChoice { get; private set; }

    public CloseConfirmationDialog()
    {
        InitializeComponent();
        Logger.Info("初始化 CloseConfirmationDialog 窗口");
        InitializeDefaultSettings();
        Logger.Info("设置对话框的默认设置");
        DialogHeight = 200;
        // DialogWidth = 400;
        Logger.Debug("设置对话框高度为 200");
    }

    // 初始化默认设置
    private void InitializeDefaultSettings()
    {
        RbMinimizeToTray.IsChecked = true; // 默认选择最小化到系统托盘
        Logger.Info("默认选中最小化到系统托盘");
    }

    // "确定" 按钮点击事件处理
    private void BtnOk_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击了确定按钮");
        SetDialogResults(); // 设置对话框结果
        Logger.Info("设置对话框结果");
        DialogResult = true; // 设置对话框结果为 true，表示用户确认
        Logger.Debug("对话框结果设置为 true");
    }

    // "取消" 按钮点击事件处理
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        Logger.Info("用户点击了取消按钮");
        DialogResult = false; // 设置对话框结果为 false，表示用户取消
        Logger.Debug("对话框结果设置为 false");
    }

    // 设置对话框的结果，根据用户的选择更新 IsClose 和 RememberChoice 属性
    private void SetDialogResults()
    {
        IsClose = RbClose.IsChecked == true;
        RememberChoice = CbRememberChoice.IsChecked == true;
        Logger.Info($"设置 IsClose 为 {IsClose}, RememberChoice 为 {RememberChoice}");
    }
}