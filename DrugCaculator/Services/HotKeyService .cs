using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DrugCalculator.Services
{
    public class HotKeyService : IDisposable
    {
        private static HotKeyService _instance;
        private static readonly object Lock = new();

        private const int WmHotkey = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Dictionary<int, Action> _hotKeyActions = new();
        private readonly WindowInteropHelper _interopHelper;
        private int _currentHotKeyId = 0;

        // 私有构造函数，防止外部实例化
        private HotKeyService(Window mainWindow)
        {
            _interopHelper = new WindowInteropHelper(mainWindow);
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
        }

        // 获取唯一实例
        public static HotKeyService Instance(Window mainWindow)
        {
            if (_instance != null) return _instance;
            lock (Lock)
            {
                _instance ??= new HotKeyService(mainWindow);
            }
            return _instance;
        }

        public int RegisterHotKey(uint modifiers, uint key, Action action)
        {
            _currentHotKeyId++;
            if (!RegisterHotKey(_interopHelper.Handle, _currentHotKeyId, modifiers, key))
            {
                throw new InvalidOperationException("Could not register hot key.");
            }

            _hotKeyActions[_currentHotKeyId] = action;
            return _currentHotKeyId;
        }

        public void UnregisterHotKey(int hotKeyId)
        {
            UnregisterHotKey(_interopHelper.Handle, hotKeyId);
            _hotKeyActions.Remove(hotKeyId);
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (msg.message != WmHotkey) return;
            var hotKeyId = msg.wParam.ToInt32();
            if (!_hotKeyActions.TryGetValue(hotKeyId, out var action)) return;
            action?.Invoke();
            handled = true;
        }

        public void Dispose()
        {
            foreach (var hotKeyId in _hotKeyActions.Keys)
            {
                UnregisterHotKey(hotKeyId);
            }
            _hotKeyActions.Clear();
            ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
        }
    }
}
