using System.Threading;
using System.Windows;

namespace DrugCaculator
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static Mutex Mutex1 { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "DrugCalculator_SingleInstanceMutex";

            Mutex1 = new Mutex(true, mutexName, out var createdNew);

            if (!createdNew)
            {
                // 如果应用程序的实例已经存在，则退出新实例
                MessageBox.Show("应用程序已经在运行中。");
                Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }
    }
}
