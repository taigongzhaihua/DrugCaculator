using IWshRuntimeLibrary;
using System;
using System.IO;
using File = System.IO.File;

// 这里实现了开启和关闭开机启动的功能，由于采用的是vbs脚本形式实现，
// 所以程序可能会被某些杀毒软件误报，但是不影响程序的正常运行，后期
// 会考虑其他方式实现开机启动功能
namespace DrugCalculator.Services
{
    public class StartupService
    {
        // 静态公共方法：设置开机启动
        public static void SetStartup()
        {
            // 获取当前目录的路径
            var currentDirectory = Directory.GetCurrentDirectory();

            // 定义 VBS 文件路径和快捷方式路径
            var vbsFilePath = Path.Combine(currentDirectory, "StartApp.vbs");
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DrugCalculator.lnk");

            // 创建 VBS 文件
            CreateVbsFile(vbsFilePath);

            // 创建快捷方式
            CreateShortcut(shortcutPath, vbsFilePath);

            Console.WriteLine(@"VBS 文件和快捷方式已成功创建到启动菜单。");
        }

        // 静态公共方法：删除开机启动
        public static void RemoveStartup()
        {
            // 获取当前目录的路径
            var currentDirectory = Directory.GetCurrentDirectory();

            // 定义 VBS 文件路径和快捷方式路径
            var vbsFilePath = Path.Combine(currentDirectory, "StartApp.vbs");
            var shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "DrugCalculator.lnk");

            // 删除 VBS 文件
            if (File.Exists(vbsFilePath))
            {
                File.Delete(vbsFilePath);
                Console.WriteLine(@"VBS 文件已删除。");
            }

            // 删除快捷方式
            if (!File.Exists(shortcutPath)) return;
            File.Delete(shortcutPath);
            Console.WriteLine(@"快捷方式已删除。");
        }

        // 私有静态方法：创建 VBS 文件
        private static void CreateVbsFile(string vbsFilePath)
        {
            // VBS 脚本内容，用于运行 DrugCalculator.exe 并隐藏窗口
            const string vbsContent = "Set WshShell = CreateObject(\"WScript.Shell\")\n" +
                                      "WshShell.Run \"\"\"DrugCalculator.exe\"\"\", 0, False";

            // 将 VBS 内容写入文件
            File.WriteAllText(vbsFilePath, vbsContent);
            Console.WriteLine(@"VBS 文件已创建。");
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
            Console.WriteLine(@"快捷方式已创建，并添加到启动菜单。");
        }
    }
}
