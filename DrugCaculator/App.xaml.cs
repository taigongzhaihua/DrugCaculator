using DrugCaculator.View.Components;
using System.Threading;
using System.Windows;

namespace DrugCaculator;
public partial class App
{
    public static Mutex Mutex1 { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        // 创建一个命名互斥体，以确保只有一个应用程序实例在运行
        const string mutexName = "DrugCalculator_SingleInstanceMutex";
        Mutex1 = new Mutex(true, mutexName, out var createdNew);

        // 如果应用程序的实例已经存在，则退出新实例
        if (!createdNew)
        {
            CustomMessageBox.Show("应用程序已经在运行中。", "警告", MsgBoxButtons.Ok, MsgBoxIcon.Warning);
            Current.Shutdown();
            return;
        }
        base.OnStartup(e);
    }
}