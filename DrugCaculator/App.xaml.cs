using DrugCalculator.Services;
using DrugCalculator.View.Windows;
using NLog;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DrugCalculator;

public partial class App
{
    // 创建一个静态的命名互斥体，以确保只有一个应用程序实例在运行
    public static Mutex Mutex1 { get; private set; }
    private const string MutexName = "DrugCalculator_SingleInstanceMutex"; // 互斥体名称

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // 获取当前类的日志记录器
    private TrayService _trayService; // 托盘服务
    private HotKeyService _hotKeyService; // 热键注册服务

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LoggerService.SetLogger();
        Logger.Info("应用程序启动");

        // 创建一个命名互斥体，以确保只有一个应用程序实例在运行
        Mutex1 = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            Logger.Warn("检测到已有实例正在运行，通知已存在的实例");
            PipeService.NotifyExistingInstance();
            Current.Shutdown();
            return;
        }

        // 创建管道服务端用于接收其他实例的消息
        Task.Run(PipeService.StartPipeServer);

        // 实例化托盘服务
        _trayService = TrayService.Instance;

        // 使用 Dispatcher 延迟执行，以确保窗口创建完成后再执行
        Dispatcher.InvokeAsync(() =>
        {
            if (Current.MainWindow is MainWindow mainWindow)
            {
                // 订阅主窗口的 Loaded 事件
                mainWindow.Loaded += MainWindow_Loaded;
            }
        });

    }
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not MainWindow mainWindow) return;

        // 确保 Loaded 事件只处理一次
        mainWindow.Loaded -= MainWindow_Loaded;

        // 初始化热键服务
        _hotKeyService = HotKeyService.Instance(mainWindow);

        // 注册热键以显示主窗口
        _hotKeyService.RegisterHotKey((uint)ModifierKeys.Control | (uint)ModifierKeys.Alt, (uint)System.Windows.Forms.Keys.H, () =>
        {
            if (mainWindow.Visibility != Visibility.Visible)
            {
                mainWindow.Show();
            }
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        });
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hotKeyService.Dispose();
        _trayService.Dispose();
        base.OnExit(e);
    }
}