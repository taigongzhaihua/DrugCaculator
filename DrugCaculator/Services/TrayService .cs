using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 托盘服务，用于管理应用程序的托盘图标和上下文菜单。
    /// </summary>
    public class TrayService : IDisposable
    {
        // Lazy 单例实现
        private static readonly Lazy<TrayService> LazyInstance = new(() => new TrayService());

        /// <summary>
        /// 获取 TrayService 的唯一实例。
        /// </summary>
        public static TrayService Instance => LazyInstance.Value;

        private NotifyIcon _notifyIcon; // 托盘图标
        private ContextMenu _trayContextMenu; // 上下文菜单
        private bool _disposed; // 是否已释放资源

        /// <summary>
        /// 私有构造函数，防止外部直接实例化。
        /// </summary>
        private TrayService()
        {
            CreateNotifyIcon();
        }

        /// <summary>
        /// 创建托盘图标。
        /// </summary>
        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = LoadIcon("AppIcon.ico"), // 动态加载图标
                Visible = true,
                Text = @"药物查询" // 鼠标悬停文本
            };
            _notifyIcon.MouseUp += NotifyIcon_MouseUp;
        }

        /// <summary>
        /// 加载托盘图标。
        /// </summary>
        /// <param name="iconPath">图标文件路径。</param>
        /// <returns>加载的图标。</returns>
        private static Icon LoadIcon(string iconPath)
        {
            try
            {
                return new Icon(iconPath);
            }
            catch (Exception)
            {
                // 提供一个默认图标，防止图标文件丢失导致异常
                return SystemIcons.Application;
            }
        }

        /// <summary>
        /// 托盘图标鼠标事件处理。
        /// </summary>
        private void NotifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Right:
                    ShowContextMenu();
                    break;
                case MouseButtons.Left:
                    ShowMainWindow();
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 显示托盘上下文菜单。
        /// </summary>
        private void ShowContextMenu()
        {
            if (_trayContextMenu == null)
                CreateContextMenu();

            if (_trayContextMenu == null) return;

            _trayContextMenu.IsOpen = !_trayContextMenu.IsOpen;
            _trayContextMenu.Placement = PlacementMode.MousePoint;
            _trayContextMenu.PlacementTarget = null;
        }

        /// <summary>
        /// 创建托盘上下文菜单。
        /// </summary>
        private void CreateContextMenu()
        {
            _trayContextMenu = new ContextMenu();

            var menuItems = GetMenuItems();
            foreach (var item in menuItems)
            {
                _trayContextMenu.Items.Add(item);
            }
        }

        /// <summary>
        /// 动态获取菜单项。
        /// </summary>
        /// <returns>菜单项集合。</returns>
        private IEnumerable<MenuItem> GetMenuItems()
        {
            yield return CreateMenuItem("打开", "\ue69b", (_, _) => ShowMainWindow());
            yield return CreateMenuItem("退出", "\ue600", (_, _) => ExitApplication());
        }

        /// <summary>
        /// 创建菜单项。
        /// </summary>
        /// <param name="header">菜单项文本。</param>
        /// <param name="icon">菜单项图标（字符串表示）。</param>
        /// <param name="clickHandler">点击事件处理程序。</param>
        /// <returns>创建的菜单项。</returns>
        private static MenuItem CreateMenuItem(string header, string icon, RoutedEventHandler clickHandler)
        {
            var menuItem = new MenuItem { Header = header, Icon = icon };
            menuItem.Click += clickHandler;
            return menuItem;
        }

        /// <summary>
        /// 显示主窗口。
        /// </summary>
        private static void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null) return;

                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Activate();
            });
        }

        /// <summary>
        /// 退出应用程序。
        /// </summary>
        private void ExitApplication()
        {
            Dispose(); // 释放资源
            Application.Current.Shutdown(); // 关闭应用程序
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 实现资源释放逻辑。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 释放托盘图标
                if (_notifyIcon != null)
                {
                    _notifyIcon.MouseUp -= NotifyIcon_MouseUp; // 取消托盘图标的事件订阅
                    _notifyIcon.Visible = false; // 隐藏托盘图标
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }

                // 释放上下文菜单
                if (_trayContextMenu != null)
                {
                    _trayContextMenu.Items.Clear();
                    _trayContextMenu = null;
                }
            }

            _disposed = true;
        }


        /// <summary>
        /// 析构函数，确保资源被释放。
        /// </summary>
        ~TrayService()
        {
            Dispose(false);
        }
    }
}
