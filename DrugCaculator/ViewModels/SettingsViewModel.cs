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

namespace DrugCalculator.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly ConfigurationService ConfigurationService = ConfigurationService.Instance;
    // 是否关闭设置的属性
    private string _isCloseSetting;

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
            Logger.Debug($"设置日志等级为{level.Name}");
            LoggerService.SetLogLevel(level);
        }
    }
    private bool _isAutoStart;
    public bool IsAutoStart
    {
        get => _isAutoStart;
        set
        {
            SetProperty(ref _isAutoStart, value);
            SaveSetting(ref value, "IsAutoStart");
            if (value)
            {
                StartupService.SetStartup();
            }
            else
            {
                StartupService.RemoveStartup();
            }
        }
    }
    // 配置 API 密钥的命令
    public ICommand ConfigureApiKeyCommand { get; set; }
    public List<string> LogLevelOptions { get; set; }

    // 构造函数
    public SettingsViewModel()
    {
        Logger.Info("初始化 SettingsViewModel");
        LoadSetting(out _isCloseSetting, "IsClose");
        LoadSetting(out _isAutoStart, "IsAutoStart");
        LogLevelOptions = ConfigurationService.GetOption("LogLevelOptions");
        LogLevelValue = LoggerService.GetSavedLogLevel().Ordinal;
        ConfigureApiKeyCommand = new RelayCommand(OpenApiKeyDialog);
    }

    // 加载设置
    private static void LoadSetting<T>(out T property, string settingName)
    {
        Logger.Info("从设置中加载 IsClose 设置");
        property = (T)Properties.Settings.Default[settingName];
    }

    // 保存设置
    private static void SaveSetting<T>(ref T property, string settingName)
    {
        Properties.Settings.Default[settingName] = property;
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

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, newValue)) return false;
        field = newValue;
        OnPropertyChanged(propertyName);
        return true;
    }
}