using IWshRuntimeLibrary;
using NLog;
using System;
using System.IO;
using File = System.IO.File;

// 这里实现了开启和关闭开机启动的功能，采用启动菜单注入快捷方式的方式实现，测试第一次操作会被360报毒，但允许一次后，后续没有再出现。
namespace DrugCalculator.Services;

public class StartupService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    // 静态公共方法：设置开机启动
    public static void SetStartup()
    {
        // 获取当前目录的路径
        var currentDirectory = Directory.GetCurrentDirectory();

        // 定义程序路径和快捷方式路径
        var targetPath = Path.Combine(currentDirectory, "DrugCalculator.exe");
        var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DrugCalculator.lnk");

        // 创建快捷方式
        CreateShortcut(shortcutPath, targetPath);
        Logger.Info("设置开机启动成功！");
    }

    // 静态公共方法：删除开机启动
    public static void RemoveStartup()
    {
        // 定义快捷方式路径
        var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DrugCalculator.lnk");

        // 删除快捷方式
        if (!File.Exists(shortcutPath)) return;
        File.Delete(shortcutPath);
        Logger.Info("删除开机启动成功！");
    }

    // 私有静态方法：创建快捷方式
    private static void CreateShortcut(string shortcutPath, string targetPath)
    {
        // 创建 WshShell 对象
        var shell = new WshShell();

        // 创建快捷方式对象
        var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

        // 设置快捷方式的目标路径（即 VBS 文件的路径）
        shortcut.TargetPath = targetPath;

        // 设置快捷方式的工作目录为当前目录
        shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);

        // 设置快捷方式的窗口风格为正常窗口（1 表示正常窗口）
        shortcut.WindowStyle = 1;

        // 保存快捷方式
        shortcut.Save();
        Logger.Info("创建快捷方式成功！");
    }
}