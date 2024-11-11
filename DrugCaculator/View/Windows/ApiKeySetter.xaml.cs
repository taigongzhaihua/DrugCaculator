using DrugCaculator.View.Controls;
using DrugCaculator.ViewModels;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DrugCaculator.View.Windows;

public partial class ApiKeySetter
{
    public ApiKeySetter()
    {
        InitializeComponent();

        // 获取单例 ViewModel 实例并设置 DataContext
        var viewModel = new ApiKeySetterViewModel();
        DataContext = viewModel;

        // 订阅关闭事件
        viewModel.OnRequestClose += Close;

        // 设置控件的绑定
        BindControl(PasswordInput, PasswordInputComponent.PasswordProperty, "ApiKey", viewModel);
        BindCommand(ConfirmButton, "ConfirmCommand", viewModel);
        BindCommand(CancelButton, "CancelCommand", viewModel);
    }

    // 统一的属性绑定方法
    private static void BindControl(FrameworkElement control, DependencyProperty property, string path, object source)
    {
        control.SetBinding(property, new Binding(path) { Source = source, Mode = BindingMode.TwoWay });
    }

    // 统一的命令绑定方法
    private static void BindCommand(ButtonBase button, string commandPath, object source)
    {
        BindingOperations.SetBinding(button, ButtonBase.CommandProperty, new Binding(commandPath) { Source = source });
    }
}