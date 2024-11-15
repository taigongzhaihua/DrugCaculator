using DrugCalculator.Properties;
using NLog;
using System;
using System.Collections.Generic;

namespace DrugCalculator.Services;

public static class LoggerService
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void SetLogger()
    {
        var config = new NLog.Config.LoggingConfiguration();

        // 设置日志输出到文件
        var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "Log.log" };

        // 配置日志级别和输出目标
        var minLevel = GetSavedLogLevel();
        config.AddRule(minLevel, LogLevel.Fatal, logfile);

        // 应用配置
        LogManager.Configuration = config;
    }

    public static void SetLogLevel(LogLevel minLevel)
    {
        var config = LogManager.Configuration;
        if (config != null)
        {
            foreach (var rule in config.LoggingRules) rule.SetLoggingLevels(minLevel, LogLevel.Fatal);
            LogManager.ReconfigExistingLoggers();
        }

        SaveLogLevel(minLevel);
    }

    private static void SaveLogLevel(LogLevel minLevel)
    {
        Settings.Default.LogLevel = minLevel.Name;
        Settings.Default.Save();
        Logger.Debug($"设置日志等级为{minLevel.Name}");
    }

    private static readonly Dictionary<string, LogLevel> LogLevelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Info", LogLevel.Info },
        { "Debug", LogLevel.Debug },
        { "Error", LogLevel.Error },
        { "Warn", LogLevel.Warn },
        { "Trace", LogLevel.Trace }
    };

    public static LogLevel GetSavedLogLevel()
    {
        var savedLogLevelString = Settings.Default.LogLevel;

        return LogLevelMap.TryGetValue(savedLogLevelString, out var logLevel) ? logLevel : LogLevel.Info; // 默认级别
    }
}