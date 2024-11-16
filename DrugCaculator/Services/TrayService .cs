using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;
// WPF的ContextMenu

namespace DrugCalculator.Services
{
    public class TrayService : IDisposable
    {
        private bool _disposed = false;

        private static TrayService _instance;
        private static readonly object Lock = new();

        private NotifyIcon _notifyIcon;
        private ContextMenu _trayContextMenu;

        // 私有构造函数，防止外部实例化
        private TrayService()
        {
            CreateNotifyIcon();
            CreateContextMenu();
        }

        // 获取唯一实例
        public static TrayService Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    _instance ??= new TrayService();
                }
                return _instance;
            }
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("AppIcon.ico"),
                Visible = true
            };
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;
        }

        private void CreateContextMenu()
        {
            _trayContextMenu = new ContextMenu();
            var openMenuItem = new MenuItem { Header = "打开", Icon = "\ue69b" };
            openMenuItem.Click += Open_Click;

            var exitMenuItem = new MenuItem { Header = "退出", Icon = "\ue600" };
            exitMenuItem.Click += Exit_Click;

            _trayContextMenu.Items.Add(openMenuItem);
            _trayContextMenu.Items.Add(exitMenuItem);
        }

        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu();
            }
            else if (e.Button == MouseButtons.Left)
            {
                ShowMainWindow();
            }
        }

        private void ShowContextMenu()
        {
            if (_trayContextMenu != null)
            {
                _trayContextMenu.IsOpen = true;
                _trayContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                _trayContextMenu.PlacementTarget = null;
            }
        }

        private static void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.MainWindow!.Show();
                Application.Current.MainWindow.WindowState = WindowState.Normal;
                Application.Current.MainWindow.Activate();
            });
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        // Dispose 方法
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 真正的资源释放方法
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // 释放托管资源
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }

            // 释放非托管资源（如果有）
            _disposed = true;
        }

        // 析构函数（终结器）
        ~TrayService()
        {
            Dispose(false);
        }
    }
}
