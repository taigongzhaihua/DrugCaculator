using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 提供全局热键注册和管理服务的类，支持单例模式。
    /// </summary>
    public partial class HotKeyService : IDisposable
    {
        // 消息类型：全局热键消息
        private const int WmHotkey = 0x0312;

        // 导入 user32.dll 中的 RegisterHotKey 函数，用于注册全局热键
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // 导入 user32.dll 中的 UnregisterHotKey 函数，用于注销全局热键
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial void UnregisterHotKey(IntPtr hWnd, int id);

        // 保存已注册热键的字典：键为热键 ID，值为对应的回调函数
        private readonly Dictionary<int, Action> _hotKeyActions = [];

        // 用于获取窗口句柄的辅助类
        private readonly WindowInteropHelper _interopHelper;

        // 当前的热键 ID（用于生成唯一的热键 ID）
        private int _currentHotKeyId;

        // 表示对象是否已被释放
        private bool _disposed;

        // Lazy 初始化单例实例，确保线程安全
        private static readonly Lazy<HotKeyService> LazyInstance = new(() =>
            new HotKeyService(Application.Current.MainWindow));

        /// <summary>
        /// 获取 HotKeyService 的唯一实例。
        /// </summary>
        public static HotKeyService Instance => LazyInstance.Value;

        /// <summary>
        /// 私有构造函数，防止外部直接实例化。
        /// 初始化热键服务并订阅全局消息预处理事件。
        /// </summary>
        /// <param name="mainWindow">主窗口对象，用于获取窗口句柄。</param>
        private HotKeyService(Window mainWindow)
        {
            ArgumentNullException.ThrowIfNull(mainWindow);

            // 初始化 WindowInteropHelper，用于获取窗口的句柄
            _interopHelper = new WindowInteropHelper(mainWindow);

            // 订阅全局线程消息预处理事件，用于捕获全局热键消息
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        /// <summary>
        /// 注册全局热键，并关联一个回调函数。
        /// </summary>
        /// <param name="modifiers">修饰键，例如 Ctrl、Alt 等，使用位标志组合。</param>
        /// <param name="key">需要监听的键，例如 A、B 等。</param>
        /// <param name="action">热键触发时执行的回调函数。</param>
        /// <returns>热键的唯一 ID。</returns>
        /// <exception cref="InvalidOperationException">当热键注册失败时抛出异常。</exception>
        public int RegisterHotKey(uint modifiers, uint key, Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            // 生成唯一的热键 ID
            _currentHotKeyId++;

            // 调用 Windows API 注册热键
            if (!RegisterHotKey(_interopHelper.Handle, _currentHotKeyId, modifiers, key))
            {
                throw new InvalidOperationException("Failed to register hot key.");
            }

            // 将热键 ID 和回调函数存入字典
            _hotKeyActions[_currentHotKeyId] = action;
            return _currentHotKeyId;
        }

        /// <summary>
        /// 注销指定的全局热键。
        /// </summary>
        /// <param name="hotKeyId">热键的唯一 ID。</param>
        public void UnregisterHotKey(int hotKeyId)
        {
            // 如果热键 ID 存在，则注销对应热键
            if (_hotKeyActions.Remove(hotKeyId))
            {
                UnregisterHotKey(_interopHelper.Handle, hotKeyId);
            }
        }

        /// <summary>
        /// 全局消息预处理逻辑，用于处理热键触发的消息。
        /// </summary>
        /// <param name="msg">包含消息数据的结构体。</param>
        /// <param name="handled">是否已处理该消息。</param>
        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            // 检查消息类型是否为热键
            if (msg.message == WmHotkey)
            {
                // 获取热键 ID，并尝试从字典中获取对应的回调函数
                var hotKeyId = msg.wParam.ToInt32();
                if (_hotKeyActions.TryGetValue(hotKeyId, out var action))
                {
                    // 执行回调函数
                    action?.Invoke();
                    handled = true; // 标记消息已处理
                }
            }
        }

        /// <summary>
        /// 释放资源，注销所有热键并取消事件订阅。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 真正的资源释放逻辑，区分托管资源和非托管资源。
        /// </summary>
        /// <param name="disposing">是否需要释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 注销所有已注册的热键
                foreach (var hotKeyId in _hotKeyActions.Keys)
                {
                    UnregisterHotKey(hotKeyId);
                }

                // 清空热键字典
                _hotKeyActions.Clear();

                // 取消订阅全局消息预处理事件
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            }

            _disposed = true;
        }

        /// <summary>
        /// 析构函数，用于确保未显式释放时仍能释放资源。
        /// </summary>
        ~HotKeyService()
        {
            Dispose(false);
        }
    }
}

