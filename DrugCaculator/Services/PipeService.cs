using NLog;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 管道服务，用于实现单实例通信机制。
    /// </summary>
    public static partial class PipeService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string PipeName = "DrugCalculator_SingleInstancePipe";
        private static CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// 通知已存在的实例。
        /// 如果管道服务器已存在，则尝试连接，并发送消息通知它显示主窗口。
        /// </summary>
        public static void NotifyExistingInstance()
        {
            var connected = false;
            var retries = 3; // 最大重试次数

            while (!connected && retries > 0)
            {
                try
                {
                    Logger.Info("尝试连接已有实例...");
                    using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                    client.Connect(1000); // 尝试连接到已有实例，超时时间为1000毫秒

                    using var writer = new StreamWriter(client);
                    writer.WriteLine("SHOW"); // 向服务器发送指令
                    writer.Flush();

                    connected = true;
                    Logger.Info("成功连接到已有实例，并发送消息。");
                }
                catch (Exception ex)
                {
                    retries--;
                    Logger.Warn($"连接失败，剩余重试次数: {retries}");
                    Logger.Error($"连接已有实例时发生异常：{ex.Message}", ex);

                    if (retries > 0)
                        Thread.Sleep(500); // 短暂等待后重试
                }
            }

            if (!connected)
            {
                Logger.Error("多次尝试后仍无法连接到已有实例，操作失败。");
            }
        }

        /// <summary>
        /// 启动管道服务器。
        /// 持续监听管道连接，接收其他实例的消息并进行处理。
        /// </summary>
        public static void StartPipeServer()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Logger.Info("管道服务器已启动，等待连接...");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 创建命名管道服务端
                    using var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message);

                    server.WaitForConnection(); // 等待客户端连接
                    Logger.Info("客户端已连接到管道。");

                    // 读取客户端发送的消息
                    using var reader = new StreamReader(server);
                    var message = reader.ReadLine();

                    if (message == "SHOW")
                    {
                        Logger.Info("接收到 SHOW 消息，准备显示主窗口。");
                        ShowMainWindow();
                    }

                    server.Disconnect(); // 断开当前连接，准备接收下一个连接
                    Logger.Info("管道连接已断开。");
                }
                catch (ObjectDisposedException ex)
                {
                    Logger.Warn($"管道服务器已关闭：{ex.Message}");
                    break; // 管道已关闭，退出循环
                }
                catch (Exception ex)
                {
                    Logger.Error($"管道服务器发生异常：{ex.Message}", ex);
                }
            }

            Logger.Info("管道服务器已停止运行。");
        }

        /// <summary>
        /// 停止管道服务器。
        /// 通过取消令牌通知服务器退出。
        /// </summary>
        public static void StopPipeServer()
        {
            try
            {
                Logger.Info("正在停止管道服务器...");
                _cancellationTokenSource?.Cancel();
                Logger.Info("管道服务器已成功停止。");
            }
            catch (Exception ex)
            {
                Logger.Error($"停止管道服务器时发生异常：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 显示主窗口。
        /// 当接收到 "SHOW" 消息时调用，用于激活和显示主窗口。
        /// </summary>
        private static void ShowMainWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow == null)
                {
                    Logger.Warn("主窗口未初始化，无法显示。");
                    return;
                }

                mainWindow.Show(); // 显示窗口
                mainWindow.WindowState = WindowState.Normal; // 恢复窗口状态
                mainWindow.Activate(); // 激活窗口

                // 将窗口置于最前
                var hwnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
                SetForegroundWindow(hwnd);
            });
        }

        /// <summary>
        /// 将窗口置于最前。
        /// </summary>
        /// <param name="hWnd">窗口句柄。</param>
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial void SetForegroundWindow(IntPtr hWnd);
    }
}
