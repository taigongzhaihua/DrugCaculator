using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace DrugCalculator.Services;

public class TrayService : IDisposable
{
    private bool _disposed;

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

    // 创建托盘图标
    private void CreateNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = new Icon("AppIcon.ico"),
            Visible = true
        };
        _notifyIcon.MouseUp += NotifyIcon_MouseUp;
    }

    // 创建上下文菜单
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

    // 托盘图标鼠标事件处理
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
            case MouseButtons.Middle:
            case MouseButtons.XButton1:
            case MouseButtons.XButton2:
            default:
                break;
        }
    }

    // 显示托盘上下文菜单
    private void ShowContextMenu()
    {
        if (_trayContextMenu == null) return;
        _trayContextMenu.IsOpen = !_trayContextMenu.IsOpen;
        _trayContextMenu.Placement = PlacementMode.MousePoint;
        _trayContextMenu.PlacementTarget = null;
    }

    // 显示主窗口
    private static void ShowMainWindow()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            Application.Current.MainWindow!.Show();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.Activate();
        });
    }

    // 打开菜单项点击事件
    private static void Open_Click(object sender, RoutedEventArgs e)
    {
        ShowMainWindow();
    }

    // 退出菜单项点击事件
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Dispose(); // 调用 Dispose() 方法释放资源
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
                _notifyIcon.MouseUp -= NotifyIcon_MouseUp; // 取消事件订阅
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            if (_trayContextMenu != null)
            {
                // 取消上下文菜单项的事件订阅
                foreach (var item in _trayContextMenu.Items)
                {
                    if (item is not MenuItem menuItem) continue;
                    menuItem.Click -= Open_Click;
                    menuItem.Click -= Exit_Click;
                }
                _trayContextMenu = null; // 释放上下文菜单
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