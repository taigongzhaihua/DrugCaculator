using NLog;

namespace DrugCaculator.View.Windows;

public partial class SettingsWindow
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public SettingsWindow()
    {
        InitializeComponent();
        Logger.Info("初始化 SettingsWindow 窗口");
    }
}