using DrugCalculator.Properties;
using DrugCalculator.Services;
using DrugCalculator.Utilities.Commands;
using DrugCalculator.View.Components;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// ReSharper disable UnusedMember.Global

namespace DrugCalculator.ViewModels;

public class ApiKeySetterViewModel : INotifyPropertyChanged
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private string _apiKey;

    public string ApiKey
    {
        get => _apiKey;
        set => SetField(ref _apiKey, value);
    }

    public ApiKeySetterViewModel()
    {
        Logger.Info("初始化 ApiKeySetterViewModel");
        LoadApiKey();
    }

    // 从设置中加载 API 密钥
    private void LoadApiKey()
    {
        try
        {
            ApiKey = DeepSeekService.GetApiKeyFromSettings();
            Logger.Info("API 密钥成功从设置中获取。");
        }
        catch (Exception ex)
        {
            Logger.Error("从设置中获取 API 密钥时发生错误。", ex);
            CustomMessageBox.Show("无法从设置中获取 API 密钥。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    private ICommand _confirmCommand;
    public ICommand ConfirmCommand => _confirmCommand ??= new RelayCommand(ConfirmApiKey);

    // 确认并保存 API 密钥
    private void ConfirmApiKey(object o)
    {
        var window = Window.GetWindow((o as Button)!);
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            Logger.Warn("尝试保存空的 API 密钥。");
            CustomMessageBox.Show(window, "API 密钥不能为空。", "警告", MsgBoxButtons.Ok, MsgBoxIcon.Warning);
            return;
        }

        try
        {
            var encryptedApiKey = EncryptionService.Encrypt(ApiKey, "DeepSeekApiKey");
            Settings.Default.DeepSeekApiKey = encryptedApiKey;
            Settings.Default.Save();

            Logger.Info("API 密钥已成功加密并保存到设置中。");
            CustomMessageBox.Show(window, "API 密钥已成功保存。", "成功", MsgBoxButtons.Ok, MsgBoxIcon.Success);
            OnRequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.Error("保存 API 密钥时发生错误。", ex);
            CustomMessageBox.Show(window, $"无法保存 API 密钥:{ex.Message}", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    private ICommand _cancelCommand;
    public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(_ => Cancel());

    // 取消操作
    private void Cancel()
    {
        Logger.Info("用户取消了 API 密钥设置。");
        OnRequestClose?.Invoke();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    // 属性变化通知
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        Logger.Debug($"属性 {propertyName} 发生变化。");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // 设置字段并触发属性变化通知
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // 请求关闭窗口的事件
    public event Action OnRequestClose;
}