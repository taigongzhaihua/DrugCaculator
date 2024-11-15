using DrugCalculator.Services;
using NLog;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DrugCalculator;

public partial class App
{
    public static Mutex Mutex1 { get; private set; }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string MutexName = "DrugCalculator_SingleInstanceMutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        LoggerService.SetLogger();
        Logger.Info("应用程序启动");

        // 创建一个命名互斥体，以确保只有一个应用程序实例在运行
        Mutex1 = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            Logger.Warn("检测到已有实例正在运行，通知已存在的实例");
            // 如果已经存在实例，则通过管道通知已存在的实例
            PipeService.NotifyExistingInstance();
            Current.Shutdown();
            return;
        }

        // 创建管道服务端用于接收其他实例的消息
        Task.Run(PipeService.StartPipeServer);

        base.OnStartup(e);
    }
}