using DrugCaculator.Models;
using DrugCaculator.Services;
using DrugCaculator.ViewModels;

namespace DrugCaculator.View.Windows;
public partial class DrugEditor
{
    // 构造函数
    public DrugEditor(Drug drug, DrugService drugService)
    {
        InitializeComponent();

        var viewModel = new DrugEditorViewModel(drug, drugService)
        {
            CloseAction = () => DialogResult = true // 设置关闭动作
        };
        DataContext = viewModel;
    }
}