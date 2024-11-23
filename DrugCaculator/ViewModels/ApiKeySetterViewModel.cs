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

namespace DrugCalculator.ViewModels
{
    /// <summary>
    /// ViewModel 类，用于管理 API 密钥设置的逻辑。
    /// 实现了 INotifyPropertyChanged 接口，以支持属性绑定。
    /// </summary>
    public class ApiKeySetterViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private string _apiKey;

        /// <summary>
        /// API 密钥的绑定属性，用于存储和更新用户输入的密钥。
        /// </summary>
        public string ApiKey
        {
            get => _apiKey;
            set => SetField(ref _apiKey, value);
        }

        /// <summary>
        /// 构造函数，初始化 ViewModel 并加载已有的 API 密钥。
        /// </summary>
        public ApiKeySetterViewModel()
        {
            Logger.Info("初始化 ApiKeySetterViewModel");
            LoadApiKey();
        }

        /// <summary>
        /// 从应用程序设置中加载 API 密钥。
        /// </summary>
        private void LoadApiKey()
        {
            try
            {
                // 调用 DeepSeekService 从加密存储中获取 API 密钥
                ApiKey = DeepSeekService.GetApiKeyFromSettings();
                Logger.Info("API 密钥成功从设置中获取。");
            }
            catch (Exception ex)
            {
                // 如果加载失败，记录日志并弹出错误消息框
                Logger.Error($"从设置中获取 API 密钥时发生错误: {ex.Message}");
                CustomMessageBox.Show("无法从设置中获取 API 密钥。", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
            }
        }

        private ICommand _confirmCommand;

        /// <summary>
        /// 确认并保存 API 密钥的命令。
        /// </summary>
        public ICommand ConfirmCommand => _confirmCommand ??= new RelayCommand(ConfirmApiKey);

        /// <summary>
        /// 确认并保存 API 密钥。
        /// </summary>
        /// <param name="o">命令的触发源，一般为绑定的控件对象。</param>
        private void ConfirmApiKey(object o)
        {
            // 获取触发命令的窗口对象
            var window = Window.GetWindow((o as Button)!);

            // 检查 API 密钥是否为空
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                Logger.Warn("尝试保存空的 API 密钥。");
                CustomMessageBox.Show(window, "API 密钥不能为空。", "警告", MsgBoxButtons.Ok, MsgBoxIcon.Warning);
                return;
            }

            try
            {
                // 加密 API 密钥并保存到设置
                var encryptedApiKey = EncryptionService.Encrypt(ApiKey, "DeepSeekApiKey");
                Settings.Default.DeepSeekApiKey = encryptedApiKey;
                Settings.Default.Save();

                Logger.Info("API 密钥已成功加密并保存到设置中。");
                // 显示保存成功的消息框
                CustomMessageBox.Show(window, "API 密钥已成功保存。", "成功", MsgBoxButtons.Ok, MsgBoxIcon.Success);

                // 触发关闭窗口的事件
                OnRequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                // 如果保存失败，记录日志并弹出错误消息框
                Logger.Error($"保存 API 密钥时发生错误: {ex.Message}");
                CustomMessageBox.Show(window, $"无法保存 API 密钥: {ex.Message}", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
            }
        }

        private ICommand _cancelCommand;

        /// <summary>
        /// 取消操作的命令。
        /// </summary>
        public ICommand CancelCommand => _cancelCommand ??= new RelayCommand(_ => Cancel());

        /// <summary>
        /// 取消 API 密钥设置操作。
        /// </summary>
        private void Cancel()
        {
            Logger.Info("用户取消了 API 密钥设置。");
            // 触发关闭窗口的事件
            OnRequestClose?.Invoke();
        }

        /// <summary>
        /// 实现 INotifyPropertyChanged 接口，用于通知绑定系统属性值的变化。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变化通知事件。
        /// </summary>
        /// <param name="propertyName">发生变化的属性名称，默认由调用方法自动推断。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Logger.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置字段的值并触发属性变化通知。
        /// </summary>
        /// <typeparam name="T">字段的类型。</typeparam>
        /// <param name="field">字段的引用。</param>
        /// <param name="value">新的值。</param>
        /// <param name="propertyName">发生变化的属性名称，默认由调用方法自动推断。</param>
        /// <returns>如果字段的值被更改，则返回 true；否则返回 false。</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // 如果新值与旧值相同，则不执行任何操作
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            // 更新字段的值
            field = value;

            // 触发属性变化通知事件
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 请求关闭窗口的事件。
        /// </summary>
        public event Action OnRequestClose;
    }
}
