using System;
using System.Runtime.InteropServices;

namespace DrugCaculator.Utilities.Helpers
{
    public static class HotKeyHelper
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static bool Register(IntPtr handle, int hotkeyId, int modifiers, int key)
        {
            return RegisterHotKey(handle, hotkeyId, modifiers, key);
        }

        public static bool Unregister(IntPtr handle, int hotkeyId)
        {
            return UnregisterHotKey(handle, hotkeyId);
        }
    }
}