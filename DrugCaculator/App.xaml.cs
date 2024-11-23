using DrugCalculator.Services;
using DrugCalculator.View.Windows;
using NLog;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DrugCalculator
{
    /// <summary>
    /// 应用程序的入口类，继承自 WPF 的 Application 类。
    /// 用于管理应用程序的生命周期、初始化服务、处理单实例限制等。
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// 静态的命名互斥体，用于确保只有一个应用程序实例在运行。
        /// </summary>
        public static Mutex Mutex1 { get; private set; }

        private const string MutexName = "DrugCalculator_SingleInstanceMutex"; // 互斥体名称，用于唯一标识应用实例。

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // 获取当前类的日志记录器。
        private TrayService _trayService; // 托盘服务实例，用于管理系统托盘功能。
        private HotKeyService _hotKeyService; // 热键注册服务实例，用于注册和管理全局热键。

        /// <summary>
        /// 重写 OnStartup 方法，应用程序启动时执行。
        /// </summary>
        /// <param name="e">包含启动事件的参数。</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 配置日志记录服务。
            LoggerService.SetLogger();
            Logger.Info("应用程序启动");

            // 创建一个命名互斥体，以确保只有一个应用程序实例在运行。
            Mutex1 = new Mutex(true, MutexName, out var createdNew);

            if (!createdNew)
            {
                // 如果互斥体已经存在，则说明已经有一个实例在运行。
                Logger.Warn("检测到已有实例正在运行，通知已存在的实例");
                PipeService.NotifyExistingInstance(); // 通知已运行的实例激活主窗口。
                Current.Shutdown(); // 关闭当前实例。
                return;
            }

            // 启动管道服务端，用于接收来自其他实例的消息。
            Task.Run(PipeService.StartPipeServer);

            // 实例化托盘服务。
            _trayService = TrayService.Instance;

            // 使用 Dispatcher 延迟执行，确保主窗口创建完成后再执行后续操作。
            Dispatcher.InvokeAsync(() =>
            {
                if (Current.MainWindow is MainWindow mainWindow)
                {
                    // 订阅主窗口的 Loaded 事件，在窗口加载完成后进行额外初始化。
                    mainWindow.Loaded += MainWindow_Loaded;
                }
            });
        }

        /// <summary>
        /// 主窗口加载完成时的事件处理程序。
        /// </summary>
        /// <param name="sender">触发事件的对象。</param>
        /// <param name="e">事件参数。</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not MainWindow mainWindow) return;

            // 确保 Loaded 事件只处理一次。
            mainWindow.Loaded -= MainWindow_Loaded;

            // 初始化热键服务。
            _hotKeyService = HotKeyService.Instance;

            // 注册热键（Ctrl + Alt + H）以显示主窗口。
            const System.Windows.Forms.Keys h = System.Windows.Forms.Keys.H; // 设置热键为 "H" 键。
            const ModifierKeys m = ModifierKeys.Control | ModifierKeys.Alt; // 设置热键的修饰键为 Ctrl + Alt。
            _ = _hotKeyService.RegisterHotKey(
                modifiers: (uint)m,
                key: (uint)h,
                action: () =>
                {
                    // 激活主窗口。
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal; // 恢复窗口状态为正常。
                    mainWindow.Activate(); // 激活窗口并置于最前。
                }
            );
        }

        /// <summary>
        /// 重写 OnExit 方法，应用程序退出时执行。
        /// </summary>
        /// <param name="e">包含退出事件的参数。</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // 在应用程序退出时释放热键服务和托盘服务。
            _hotKeyService.Dispose();
            _trayService.Dispose();
            Logger.Info("应用程序退出");
            base.OnExit(e);
        }
    }
}
