using DrugCaculator.Utilities.Commands;
using DrugCaculator.View.Windows;
using NLog;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCaculator.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _isCloseSetting;

        // 是否关闭设置的属性
        public string IsCloseSetting
        {
            get => _isCloseSetting;
            set
            {
                if (_isCloseSetting == value) return;
                _isCloseSetting = value;
                OnPropertyChanged();
                SaveSettings();
                Logger.Debug($"IsCloseSetting 属性已更新为: {_isCloseSetting}");
            }
        }

        // 配置 API 密钥的命令
        public ICommand ConfigureApiKeyCommand { get; }

        // 构造函数
        public SettingsViewModel()
        {
            Logger.Info("初始化 SettingsViewModel");
            LoadSettings();
            ConfigureApiKeyCommand = new RelayCommand(OpenApiKeyDialog);
        }

        // 加载设置
        private void LoadSettings()
        {
            Logger.Info("从设置中加载 IsClose 设置");
            IsCloseSetting = Properties.Settings.Default.IsClose;
        }

        // 保存设置
        private void SaveSettings()
        {
            Logger.Info("保存 IsClose 设置到设置文件");
            Properties.Settings.Default.IsClose = IsCloseSetting;
            Properties.Settings.Default.Save();
        }

        // 打开 API 密钥设置对话框
        private static void OpenApiKeyDialog(object obj)
        {
            Logger.Info("打开 API 密钥设置对话框");
            var apiKeySetter = new ApiKeySetter
            {
                Owner = Window.GetWindow((obj as Button)!)
            };
            apiKeySetter.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 属性变化通知
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Logger.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
