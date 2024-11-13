using DrugCalculator.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;

namespace DrugCalculator.ViewModels;

public class LogViewerViewModel : INotifyPropertyChanged
{
    private ObservableCollection<LogEntry> _logEntries;
    private string _selectedFilter;

    public ObservableCollection<LogEntry> LogEntries
    {
        get => _logEntries;
        set
        {
            _logEntries = value;
            OnPropertyChanged(nameof(LogEntries));
        }
    }

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            _selectedFilter = value;
            OnPropertyChanged(nameof(SelectedFilter));
            CollectionViewSource.GetDefaultView(LogEntries)?.Refresh();
        }
    }

    public ICollectionView LogEntriesView { get; }

    public LogViewerViewModel()
    {
        LoadLogs();
        LogEntriesView = CollectionViewSource.GetDefaultView(LogEntries);
        LogEntriesView.Filter = LogFilter;
    }

    private void LoadLogs()
    {
        _logEntries = [];

        const string logFilePath = @"Log.log"; // NLog 日志文件路径
        if (!File.Exists(logFilePath)) return;
        try
        {
            using var fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            while (reader.ReadLine() is { } line)
            {
                var logEntry = ParseLogLine(line);
                if (logEntry != null)
                {
                    _logEntries.Add(logEntry);
                }
            }
        }
        catch (IOException ex)
        {
            // 处理异常，如弹窗通知用户或者写日志记录错误
            Console.WriteLine($@"读取日志文件时发生错误: {ex.Message}");
        }
    }


    private static LogEntry ParseLogLine(string line)
    {
        // 假设日志格式为：时间戳|日志级别|来源|消息
        var parts = line.Split('|');
        if (parts.Length >= 4)
        {
            return new LogEntry
            {
                Time = parts[0].Trim(),
                Level = parts[1].Trim(),
                Message = $"[{parts[2].Trim()}] {parts[3].Trim()}"
            };
        }
        return null;
    }

    private bool LogFilter(object item)
    {
        if (string.IsNullOrEmpty(SelectedFilter) || SelectedFilter == "全部")
        {
            return true;
        }

        var logEntry = item as LogEntry;
        return logEntry != null && logEntry.Level.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}