using IWshRuntimeLibrary;
using NLog;
using System;
using System.IO;
using File = System.IO.File;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 提供设置和删除开机启动功能的服务。
    /// 实现通过在 Windows 启动文件夹中添加或移除快捷方式的方式来控制程序的开机启动行为。
    /// </summary>
    public static class StartupService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // 常量：程序名称和快捷方式名称
        private const string ProgramName = "DrugCalculator.exe"; // 程序文件名
        private const string ShortcutName = "DrugCalculator.lnk"; // 快捷方式文件名

        /// <summary>
        /// 设置程序为开机启动。
        /// 在 Windows 启动文件夹中创建一个指向当前程序的快捷方式。
        /// </summary>
        public static void SetStartup()
        {
            try
            {
                // 获取程序路径和快捷方式路径
                var targetPath = GetProgramPath();
                var shortcutPath = GetShortcutPath();

                // 创建快捷方式
                CreateShortcut(shortcutPath, targetPath);

                // 记录成功日志
                Logger.Info("设置开机启动成功！");
            }
            catch (Exception ex)
            {
                // 捕获异常并记录错误日志
                Logger.Error(ex, "设置开机启动失败！");
            }
        }

        /// <summary>
        /// 移除程序的开机启动设置。
        /// 删除 Windows 启动文件夹中对应的快捷方式。
        /// </summary>
        public static void RemoveStartup()
        {
            try
            {
                // 获取快捷方式路径
                var shortcutPath = GetShortcutPath();

                // 检查快捷方式是否存在，如果存在则删除
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                    Logger.Info("删除开机启动成功！");
                }
                else
                {
                    // 如果快捷方式不存在，记录警告日志
                    Logger.Warn("开机启动项不存在，无需删除。");
                }
            }
            catch (Exception ex)
            {
                // 捕获异常并记录错误日志
                Logger.Error(ex, "删除开机启动失败！");
            }
        }

        /// <summary>
        /// 创建快捷方式到 Windows 启动文件夹。
        /// </summary>
        /// <param name="shortcutPath">快捷方式的完整路径。</param>
        /// <param name="targetPath">程序的完整路径。</param>
        private static void CreateShortcut(string shortcutPath, string targetPath)
        {
            try
            {
                // 使用 WshShell 创建快捷方式
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                // 设置快捷方式的目标路径，即程序路径
                shortcut.TargetPath = targetPath;

                // 设置快捷方式的工作目录为目标程序的所在目录
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);

                // 设置快捷方式窗口样式为正常窗口（1 表示正常窗口）
                shortcut.WindowStyle = 1;

                // 设置快捷方式的描述信息
                shortcut.Description = "DrugCalculator 开机启动快捷方式";

                // 保存快捷方式到指定路径
                shortcut.Save();

                // 记录成功日志
                Logger.Info("创建快捷方式成功！");
            }
            catch (Exception ex)
            {
                // 捕获异常并记录错误日志，同时将异常抛出
                Logger.Error(ex, "创建快捷方式失败！");
                throw;
            }
        }

        /// <summary>
        /// 获取程序的完整路径。
        /// 确保程序文件存在，否则抛出异常。
        /// </summary>
        /// <returns>程序路径。</returns>
        /// <exception cref="FileNotFoundException">如果程序文件不存在，将抛出此异常。</exception>
        private static string GetProgramPath()
        {
            // 获取当前程序运行目录
            var currentDirectory = Directory.GetCurrentDirectory();

            // 拼接完整的程序路径
            var programPath = Path.Combine(currentDirectory, ProgramName);

            // 检查程序文件是否存在
            if (!File.Exists(programPath))
            {
                // 如果文件不存在，抛出异常并记录错误日志
                throw new FileNotFoundException($"程序文件 {ProgramName} 未找到，请检查程序目录：{currentDirectory}");
            }

            return programPath;
        }

        /// <summary>
        /// 获取快捷方式的完整路径。
        /// </summary>
        /// <returns>快捷方式路径。</returns>
        private static string GetShortcutPath()
        {
            // 获取 Windows 启动文件夹路径，并拼接快捷方式名称
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), ShortcutName);
        }
    }
}
