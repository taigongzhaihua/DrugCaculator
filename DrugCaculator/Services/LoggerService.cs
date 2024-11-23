using DrugCalculator.Properties;
using NLog;
using System;
using System.Collections.Generic;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 日志服务类，负责管理日志配置和动态调整日志级别。
    /// </summary>
    public static class LoggerService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // 字典：将字符串映射到 NLog 的 LogLevel 对象
        private static readonly Dictionary<string, LogLevel> LogLevelMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Info", LogLevel.Info },
            { "Debug", LogLevel.Debug },
            { "Error", LogLevel.Error },
            { "Warn", LogLevel.Warn },
            { "Trace", LogLevel.Trace },
            { "Fatal", LogLevel.Fatal }
        };

        /// <summary>
        /// 初始化日志配置。
        /// 配置日志输出到文件，并设置初始日志级别。
        /// </summary>
        public static void SetLogger()
        {
            try
            {
                // 创建日志配置对象
                var config = CreateLoggingConfiguration();

                // 应用日志配置
                LogManager.Configuration = config;

                Logger.Info("日志服务初始化成功。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志服务初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 动态调整日志级别。
        /// </summary>
        /// <param name="minLevel">新的最低日志级别。</param>
        public static void SetLogLevel(LogLevel minLevel)
        {
            try
            {
                var config = LogManager.Configuration;
                if (config != null)
                {
                    // 动态调整所有日志规则的最低级别
                    foreach (var rule in config.LoggingRules)
                    {
                        rule.SetLoggingLevels(minLevel, LogLevel.Fatal);
                    }

                    LogManager.ReconfigExistingLoggers();
                }

                // 保存日志级别到设置
                SaveLogLevel(minLevel);

                Logger.Info($"日志级别已设置为 {minLevel.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "动态设置日志级别失败。");
            }
        }

        /// <summary>
        /// 创建日志配置对象。
        /// </summary>
        /// <returns>日志配置对象。</returns>
        private static NLog.Config.LoggingConfiguration CreateLoggingConfiguration()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // 配置日志输出到文件
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "Log.log", // 日志文件名称
                Layout = "${longdate}|${level}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}"
            };

            // 设置日志级别规则
            var minLevel = GetSavedLogLevel();
            config.AddRule(minLevel, LogLevel.Fatal, logfile);

            return config;
        }

        /// <summary>
        /// 获取保存的日志级别。
        /// 如果没有保存的级别或级别无效，返回默认级别 Info。
        /// </summary>
        /// <returns>日志级别。</returns>
        public static LogLevel GetSavedLogLevel()
        {
            try
            {
                var savedLogLevelString = Settings.Default.LogLevel;

                // 从字典中获取对应的日志级别
                if (LogLevelMap.TryGetValue(savedLogLevelString, out var logLevel))
                {
                    return logLevel;
                }

                // 无法解析日志级别时，记录警告并返回默认值
                Logger.Warn($"无效的日志级别配置：{savedLogLevelString}，已回退到默认级别 Info。");
                return LogLevel.Info;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "获取保存的日志级别失败，已回退到默认级别 Info。");
                return LogLevel.Info; // 默认级别
            }
        }

        /// <summary>
        /// 保存日志级别到设置文件。
        /// </summary>
        /// <param name="minLevel">要保存的日志级别。</param>
        private static void SaveLogLevel(LogLevel minLevel)
        {
            try
            {
                Settings.Default.LogLevel = minLevel.Name; // 保存日志级别名称
                Settings.Default.Save(); // 保存到设置文件
                Logger.Debug($"日志级别已保存为 {minLevel.Name}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存日志级别失败。");
            }
        }
    }
}
