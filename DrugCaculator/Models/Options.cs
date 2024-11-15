namespace DrugCalculator.Models;

// 用于表示配置文件中的具体选项
public class Options
{
    public string[] RouteOptions { get; set; }
    public string[] UnitOptions { get; set; }
    public string[] FrequencyOptions { get; set; }
    public string[] LogLevelOptions { get; set; }
}

// 用于表示配置文件的整体结构
public class AppOptions
{
    public Options Options { get; set; }
}