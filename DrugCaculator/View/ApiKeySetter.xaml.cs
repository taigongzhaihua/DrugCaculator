using DrugCaculator.View.Components;
using DrugCaculator.ViewModels;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DrugCaculator.View
{
    public partial class ApiKeySetter
    {
        public ApiKeySetter()
        {
            InitializeComponent();

            // 获取单例 ViewModel 实例
            var viewModel = ApiKeySetterViewModel.Instance;

            // 订阅关闭事件
            viewModel.OnRequestClose += Close;

            // 设置 DataContext 为 ViewModel 实例
            DataContext = viewModel;

            // 后端绑定 Password 属性到 PasswordInputComponent 控件
            var passwordBinding = new Binding("ApiKey")
            {
                Source = viewModel,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            PasswordInput.SetBinding(PasswordInputComponent.PasswordProperty, passwordBinding);

            // 后端绑定 ConfirmCommand 到 ConfirmButton
            var confirmCommandBinding = new Binding("ConfirmCommand")
            {
                Source = viewModel
            };
            BindingOperations.SetBinding(ConfirmButton, ButtonBase.CommandProperty, confirmCommandBinding);

            // 后端绑定 CancelCommand 到 CancelButton
            var cancelCommandBinding = new Binding("CancelCommand")
            {
                Source = viewModel
            };
            BindingOperations.SetBinding(CancelButton, ButtonBase.CommandProperty, cancelCommandBinding);
        }
    }
}