using DrugCalculator.Services;
using DrugCalculator.Utilities.Commands;
using DrugCalculator.View.Windows;
using NLog;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCalculator.ViewModels
{
    /// <summary>
    /// 设置窗口的视图模型，负责处理与设置相关的逻辑。
    /// 实现 INotifyPropertyChanged 接口以支持数据绑定。
    /// </summary>
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ConfigurationService ConfigurationService = ConfigurationService.Instance;

        // 是否关闭设置窗口的属性
        private string _isCloseSetting;

        /// <summary>
        /// 表示设置窗口是否关闭的属性。
        /// </summary>
        public string IsCloseSetting
        {
            get => _isCloseSetting;
            set
            {
                SetProperty(ref _isCloseSetting, value);
                SaveSetting(ref _isCloseSetting, "IsClose");
                Logger.Debug($"IsCloseSetting 属性已更新为: {_isCloseSetting}");
            }
        }

        private double _logLevelValue;

        /// <summary>
        /// 表示日志级别的数值（与滑块绑定）。
        /// </summary>
        public double LogLevelValue
        {
            get => _logLevelValue;
            set
            {
                SetProperty(ref _logLevelValue, value);
                var level = value switch
                {
                    0 => LogLevel.Trace,
                    1 => LogLevel.Debug,
                    2 => LogLevel.Info,
                    3 => LogLevel.Warn,
                    4 => LogLevel.Error,
                    5 => LogLevel.Fatal,
                    _ => LogLevel.Info
                };
                Logger.Debug($"设置日志等级为 {level.Name}");
                LoggerService.SetLogLevel(level);
            }
        }

        private bool _isAutoStart;

        /// <summary>
        /// 是否启用开机启动的属性。
        /// </summary>
        public bool IsAutoStart
        {
            get
            {
                LoadSetting(ref _isAutoStart, "IsAutoStart");
                return _isAutoStart;
            }
            set
            {
                SetProperty(ref _isAutoStart, value);
                SaveSetting(ref value, "IsAutoStart");
                Logger.Info($"IsAutoStart 属性已更新为: {value}");
            }
        }

        /// <summary>
        /// 配置 API 密钥的命令。
        /// </summary>
        public ICommand ConfigureApiKeyCommand { get; set; }

        /// <summary>
        /// 日志级别选项列表。
        /// </summary>
        public List<string> LogLevelOptions { get; set; }

        /// <summary>
        /// 设置开机启动的命令。
        /// </summary>
        public ICommand SetAutoStartCommand { get; set; }

        /// <summary>
        /// 构造函数，初始化视图模型。
        /// </summary>
        public SettingsViewModel()
        {
            Logger.Info("初始化 SettingsViewModel");

            // 从设置中加载配置项
            LoadSetting(ref _isCloseSetting, "IsClose");
            LoadSetting(ref _isAutoStart, "IsAutoStart");

            // 加载日志级别选项
            LogLevelOptions = ConfigurationService.GetOption("LogLevelOptions");
            LogLevelValue = LoggerService.GetSavedLogLevel().Ordinal;

            // 初始化命令
            ConfigureApiKeyCommand = new RelayCommand(OpenApiKeyDialog);
            SetAutoStartCommand = new RelayCommand(SetAutoStart);
        }

        /// <summary>
        /// 设置开机启动。
        /// </summary>
        private void SetAutoStart(object sender)
        {
            if (_isAutoStart)
            {
                StartupService.SetStartup();
            }
            else
            {
                StartupService.RemoveStartup();
            }
        }

        /// <summary>
        /// 从设置中加载指定的配置项。
        /// </summary>
        /// <typeparam name="T">配置项的类型。</typeparam>
        /// <param name="property">目标属性的引用。</param>
        /// <param name="settingName">配置项的名称。</param>
        private static void LoadSetting<T>(ref T property, string settingName)
        {
            Logger.Info($"从设置中加载 {settingName} 配置项");
            property = (T)Properties.Settings.Default[settingName];
        }

        /// <summary>
        /// 将指定的配置项保存到设置中。
        /// </summary>
        /// <typeparam name="T">配置项的类型。</typeparam>
        /// <param name="property">目标属性的引用。</param>
        /// <param name="settingName">配置项的名称。</param>
        private static void SaveSetting<T>(ref T property, string settingName)
        {
            Properties.Settings.Default[settingName] = property;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 打开 API 密钥设置对话框。
        /// </summary>
        /// <param name="obj">触发该命令的 UI 元素。</param>
        private static void OpenApiKeyDialog(object obj)
        {
            Logger.Info("打开 API 密钥设置对话框");

            // 打开 API 密钥设置窗口
            var apiKeySetter = new ApiKeySetter
            {
                Owner = Window.GetWindow((obj as Button)!)
            };
            apiKeySetter.ShowDialog();
        }

        /// <summary>
        /// 属性变化通知事件，当绑定的属性值发生变化时触发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知绑定系统某个属性值已发生变化。
        /// </summary>
        /// <param name="propertyName">发生变化的属性名称。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Logger.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 更新目标字段的值并触发属性变化通知。
        /// </summary>
        /// <typeparam name="T">字段的类型。</typeparam>
        /// <param name="field">目标字段的引用。</param>
        /// <param name="newValue">新的值。</param>
        /// <param name="propertyName">属性名称，用于触发变化通知。</param>
        /// <returns>如果字段值发生变化则返回 true，否则返回 false。</returns>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, newValue)) return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
