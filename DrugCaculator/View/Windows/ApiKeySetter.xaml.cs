using DrugCaculator.View.Controls;
using DrugCaculator.ViewModels;
using NLog;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DrugCaculator.View.Windows;

public partial class ApiKeySetter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ApiKeySetter()
    {
        InitializeComponent();
        Logger.Info("初始化 ApiKeySetter 窗口");

        // 获取单例 ViewModel 实例并设置 DataContext
        var viewModel = new ApiKeySetterViewModel();
        DataContext = viewModel;
        Logger.Info("设置 DataContext 为 ApiKeySetterViewModel 实例");

        // 订阅关闭事件
        viewModel.OnRequestClose += Close;
        Logger.Info("订阅 ViewModel 的 OnRequestClose 事件");

        // 设置控件的绑定
        BindControl(PasswordInput, PasswordInputComponent.PasswordProperty, "ApiKey", viewModel);
        Logger.Info("绑定 PasswordInput 控件的 PasswordProperty 到 ViewModel 的 ApiKey 属性");
        BindCommand(ConfirmButton, "ConfirmCommand", viewModel);
        Logger.Info("绑定 ConfirmButton 控件到 ViewModel 的 ConfirmCommand 命令");
        BindCommand(CancelButton, "CancelCommand", viewModel);
        Logger.Info("绑定 CancelButton 控件到 ViewModel 的 CancelCommand 命令");
    }

    // 统一的属性绑定方法
    private static void BindControl(FrameworkElement control, DependencyProperty property, string path, object source)
    {
        Logger.Debug($"绑定控件 {control.Name} 的属性 {property.Name} 到路径 {path}");
        control.SetBinding(property, new Binding(path) { Source = source, Mode = BindingMode.TwoWay });
    }

    // 统一的命令绑定方法
    private static void BindCommand(ButtonBase button, string commandPath, object source)
    {
        Logger.Debug($"绑定按钮 {button.Name} 的命令到路径 {commandPath}");
        BindingOperations.SetBinding(button, ButtonBase.CommandProperty, new Binding(commandPath) { Source = source });
    }
}
