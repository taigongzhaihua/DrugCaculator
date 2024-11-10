
using NLog;
using System;

namespace DrugCaculator.Services
{
    public class LogService
    {
        static LogService()
        {
#if !DEBUG
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: file and console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logfile.txt" };
            var console = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, console);

            // Apply config
            LogManager.Configuration = config;
#endif
        }

#if DEBUG
        private static void Log(string level, string message, Exception ex = null)
        {
            var logMessage = ex == null ? $"{level}: {message}" : $"{level}: {message}, Exception: {ex.Message} ({ex.GetType()})\nStackTrace: {ex.StackTrace}";
            Console.WriteLine(logMessage);
        }

        public static void Info(string message) => Log("INFO", message);
        public static void Warning(string message) => Log("WARNING", message);
        public static void Error(string message, Exception ex) => Log("ERROR", message, ex);
        public static void Debug(string message) => Log("DEBUG", message);
#elif RELEASE
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Info(string message) => Logger.Info(message);
        public static void Warning(string message) => Logger.Warn(message);
        public static void Error(string message, Exception ex) => Logger.Error(ex, message);
        public static void Debug(string message) => Logger.Debug(message);
#endif
    }
}