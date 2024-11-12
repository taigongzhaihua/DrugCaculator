using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DrugCaculator;
public partial class App
{
    public static Mutex Mutex1 { get; private set; }

    private const string MutexName = "DrugCalculator_SingleInstanceMutex";
    private const string PipeName = "DrugCalculator_SingleInstancePipe";

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    protected override void OnStartup(StartupEventArgs e)
    {
        // 创建一个命名互斥体，以确保只有一个应用程序实例在运行
        Mutex1 = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            // 如果已经存在实例，则通过管道通知已存在的实例
            NotifyExistingInstance();
            Current.Shutdown();
            return;
        }

        // 创建管道服务端用于接收其他实例的消息
        Task.Run(StartPipeServer);

        base.OnStartup(e);
    }

    private static void NotifyExistingInstance()
    {
        var connected = false;
        var retries = 3;

        // 重试机制，尝试多次连接管道
        while (!connected && retries > 0)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(1000); // 尝试连接已有实例
                using (var writer = new StreamWriter(client))
                {
                    writer.WriteLine("SHOW");
                    writer.Flush();
                }
                connected = true;
            }
            catch
            {
                retries--;
                Thread.Sleep(500); // 等待片刻后重试
            }
        }
    }

    private static void StartPipeServer()
    {
        while (true)
        {
            try
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances);
                server.WaitForConnection(); // 等待连接

                using (var reader = new StreamReader(server))
                {
                    var message = reader.ReadLine();
                    if (message == "SHOW")
                    {
                        Current.Dispatcher.Invoke(() =>
                        {
                            var mainWindow = Current.MainWindow;
                            if (mainWindow == null) return;
                            if (mainWindow.Visibility != Visibility.Visible)
                            {
                                mainWindow.Show();
                            }
                            if (mainWindow.WindowState == WindowState.Minimized)
                            {
                                mainWindow.WindowState = WindowState.Normal; // 恢复窗口状态
                            }
                            mainWindow.Activate(); // 激活窗口并将其置于前台
                            SetForegroundWindow(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle); // 确保窗口在最前
                        });
                    }
                }

                server.Disconnect(); // 断开连接，准备接受下一个请求
            }
            catch
            {
                // 如果发生异常，继续运行以等待下一次连接
            }
        }
    }
}

