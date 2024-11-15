using NLog;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace DrugCalculator.Services;

public static class PipeService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string PipeName = "DrugCalculator_SingleInstancePipe";

    public static void NotifyExistingInstance()
    {
        var connected = false;
        var retries = 3;

        // 重试机制，尝试多次连接管道
        while (!connected && retries > 0)
            try
            {
                Logger.Info("尝试连接已有实例");
                // 使用命名管道客户端连接到已存在的实例
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(1000); // 尝试连接已有实例，超时时间为1000毫秒
                using (var writer = new StreamWriter(client))
                {
                    writer.WriteLine("SHOW"); // 向已有实例发送消息，指示其显示窗口
                    writer.Flush();
                }

                connected = true; // 连接成功
                Logger.Info("成功连接到已有实例");
            }
            catch (Exception ex)
            {
                retries--; // 连接失败，减少重试次数
                Logger.Warn($"连接失败，剩余重试次数: {retries}");
                Logger.Error($"连接已有实例时发生异常：{ex.Message}");
                Thread.Sleep(500); // 等待片刻后重试
            }
    }

    public static void StartPipeServer()
    {
        while (true)
            try
            {
                Logger.Info("等待新的管道连接");
                // 创建命名管道服务端，用于接收其他实例的连接请求
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances);
                server.WaitForConnection(); // 等待连接

                using (var reader = new StreamReader(server))
                {
                    var message = reader.ReadLine();
                    if (message == "SHOW")
                    {
                        Logger.Info("接收到SHOW消息，准备显示主窗口");
                        // 如果接收到的消息是"SHOW"，则显示主窗口
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var mainWindow = Application.Current.MainWindow;
                            if (mainWindow == null) return;
                            mainWindow.Show(); // 显示窗口
                            mainWindow.WindowState = WindowState.Normal; // 恢复窗口状态
                            mainWindow.Activate(); // 激活窗口并将其置于前台
                            SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(mainWindow)
                                .Handle); // 确保窗口在最前
                        });
                    }
                }

                if (!server.IsConnected) continue;
                server.Disconnect(); // 断开连接，准备接受下一个请求
                Logger.Info("管道连接已断开");
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Error($"管道服务器已关闭，无法访问：{ex.Message}");
                break; // 退出循环，停止管道服务器
            }
            catch (Exception ex)
            {
                // 如果发生其他异常，继续运行以等待下一次连接
                Logger.Error($"管道服务器发生异常：{ex.Message}", ex);
            }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}