using DrugCalculator.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace DrugCalculator.ViewModels
{
    /// <summary>
    /// 日志查看器的视图模型，用于加载、过滤和展示日志文件内容。
    /// </summary>
    public class LogViewerViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<LogEntry> _logEntries;
        private string _selectedFilter;

        /// <summary>
        /// 日志条目集合，包含从日志文件加载的所有日志。
        /// </summary>
        public ObservableCollection<LogEntry> LogEntries
        {
            get => _logEntries;
            set
            {
                _logEntries = value;
                OnPropertyChanged(nameof(LogEntries)); // 通知绑定更新
            }
        }

        /// <summary>
        /// 当前选中的日志过滤器，例如按日志级别（Info, Error 等）筛选。
        /// </summary>
        public string SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                _selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter)); // 通知绑定更新
                // 刷新集合视图以应用过滤器
                CollectionViewSource.GetDefaultView(LogEntries)?.Refresh();
            }
        }

        /// <summary>
        /// 日志条目集合的视图，用于支持过滤和排序。
        /// </summary>
        public ICollectionView LogEntriesView { get; }

        /// <summary>
        /// 构造函数，初始化日志查看器视图模型。
        /// </summary>
        public LogViewerViewModel()
        {
            LoadLogs(); // 加载日志文件
            LogEntriesView = CollectionViewSource.GetDefaultView(LogEntries); // 创建集合视图
            LogEntriesView.Filter = LogFilter; // 设置过滤逻辑
        }

        /// <summary>
        /// 从日志文件加载日志条目。
        /// </summary>
        private void LoadLogs()
        {
            _logEntries = [];

            const string logFilePath = @"Log.log"; // 日志文件路径
            if (!File.Exists(logFilePath)) return; // 如果日志文件不存在则直接返回

            try
            {
                // 打开日志文件以共享模式读取，支持其他程序写入
                using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs);

                // 按行读取日志文件并解析
                while (reader.ReadLine() is { } line)
                {
                    var logEntry = ParseLogLine(line); // 解析日志行
                    if (logEntry != null) _logEntries.Add(logEntry); // 添加到日志条目集合
                }
            }
            catch (IOException ex)
            {
                // 处理读取文件时的异常，例如显示错误消息或记录日志
                Console.WriteLine($@"读取日志文件时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 解析日志行，将其转换为 `LogEntry` 对象。
        /// </summary>
        /// <param name="line">日志文件中的一行。</param>
        /// <returns>解析后的 `LogEntry` 对象，如果格式不正确返回 null。</returns>
        private static LogEntry ParseLogLine(string line)
        {
            // 假设日志格式为：时间戳|日志级别|来源|消息
            var parts = line.Split('|'); // 按 | 分割日志行
            if (parts.Length >= 4)
                return new LogEntry
                {
                    Time = parts[0].Trim(), // 时间戳部分
                    Level = parts[1].Trim(), // 日志级别部分
                    Message = $"[{parts[2].Trim()}] {parts[3].Trim()}" // 组合来源和消息部分
                };
            return null; // 如果行格式不正确则返回 null
        }

        /// <summary>
        /// 用于过滤日志条目的逻辑，根据选中的过滤条件筛选日志。
        /// </summary>
        /// <param name="item">日志条目对象。</param>
        /// <returns>如果日志条目符合过滤条件则返回 true，否则返回 false。</returns>
        private bool LogFilter(object item)
        {
            if (string.IsNullOrEmpty(SelectedFilter) || SelectedFilter == "全部") return true;

            var logEntry = item as LogEntry;
            return logEntry != null && logEntry.Level.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 属性变化通知事件，当绑定的属性值发生变化时触发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知绑定系统某个属性值已发生变化。
        /// </summary>
        /// <param name="propertyName">发生变化的属性名称，默认为调用方法的名称。</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
