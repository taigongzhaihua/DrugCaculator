using DrugCalculator.Models;
using DrugCalculator.Services;
using DrugCalculator.ViewModels;
using NLog;

namespace DrugCalculator.View.Windows;

public partial class DrugEditor
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // 构造函数
    public DrugEditor(Drug drug, DrugService drugService)
    {
        InitializeComponent();
        Logger.Info("初始化 DrugEditor 窗口");

        // 创建 ViewModel 实例并设置 DataContext
        var viewModel = new DrugEditorViewModel(drug, drugService)
        {
            CloseAction = () => DialogResult = true // 设置关闭动作
        };
        Logger.Info("创建 DrugEditorViewModel 实例并设置关闭动作");
        DataContext = viewModel;
        Logger.Info("设置 DataContext 为 DrugEditorViewModel 实例");
    }
}