using DrugCaculator.Services;
using DrugCaculator.Utilities;
using DrugCaculator.View.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Setting = DrugCaculator.Properties.Settings;
// ReSharper disable UnusedMember.Global

namespace DrugCaculator.ViewModels
{
    public class ApiKeySetterViewModel : INotifyPropertyChanged
    {

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set => SetField(ref _apiKey, value);
        }

        public ApiKeySetterViewModel()
        {
            LoadApiKey();
        }

        private void LoadApiKey()
        {
            try
            {
                ApiKey = DeepSeekService.GetApiKeyFromSettings();
                LogService.Info("API 密钥成功从设置中获取。");
            }
            catch (Exception ex)
            {
                LogService.Error("从设置中获取 API 密钥时发生错误。", ex);
                CustomMessageBox.Show("无法从设置中获取 API 密钥。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
            }
        }

        private ICommand _confirmCommand;
        public ICommand ConfirmCommand => _confirmCommand ??= new RelayCommand(ConfirmApiKey);

        private void ConfirmApiKey(object o)
        {
            var window = Window.GetWindow((o as Button)!);
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                LogService.Warning("尝试保存空的 API 密钥。");
                CustomMessageBox.Show(window, "API 密钥不能为空。", "警告", MsgBoxButtons.Ok, MsgBoxIcon.Warning);
                return;
            }

            try
            {
                var encryptedApiKey = EncryptionService.Encrypt(ApiKey, "DeepSeekApiKey");
                Setting.Default.DeepSeekApiKey = encryptedApiKey;
                Setting.Default.Save();

                LogService.Info("API 密钥已成功加密并保存到设置中。");
                CustomMessageBox.Show(window, "API 密钥已成功保存。", "成功", MsgBoxButtons.Ok, MsgBoxIcon.Success);
                OnRequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                LogService.Error("保存 API 密钥时发生错误。", ex);
                CustomMessageBox.Show(window, $"无法保存 API 密钥:{ex.Message}", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
            }
        }

        private ICommand _cancelCommand;
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(_ => Cancel());

        private void Cancel()
        {
            LogService.Info("用户取消了 API 密钥设置。");
            OnRequestClose?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            LogService.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public event Action OnRequestClose;
    }
}
