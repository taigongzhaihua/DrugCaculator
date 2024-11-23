using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace DrugCalculator.Services;

/// <summary>
/// 配置服务类，用于管理应用程序的配置文件。
/// 提供读取、修改和添加配置项的功能，采用单例模式保证全局唯一性。
/// </summary>
public sealed class ConfigurationService
{
    // 配置文件路径，默认为当前目录下的 AppOptions.json 文件
    private readonly string _configFilePath;

    /// <summary>
    /// 私有构造函数，防止外部直接实例化。
    /// 使用默认或指定的配置文件路径。
    /// </summary>
    /// <param name="configFilePath">配置文件路径，默认值为 "AppOptions.json"。</param>
    private ConfigurationService(string configFilePath = "AppOptions.json")
    {
        _configFilePath = configFilePath;
    }

    /// <summary>
    /// 静态属性，提供全局唯一的 `ConfigurationService` 实例。
    /// </summary>
    public static ConfigurationService Instance { get; } = new();

    /// <summary>
    /// 从配置文件中读取所有配置选项。
    /// </summary>
    /// <returns>包含所有配置项的字典，键为配置名称，值为配置值列表。</returns>
    public Dictionary<string, List<string>> LoadOptions()
    {
        // 读取配置文件的内容
        var jsonContent = File.ReadAllText(_configFilePath);

        // 解析 JSON 内容，定位到 "Options" 节点
        var options = JObject.Parse(jsonContent)["Options"];

        // 将 "Options" 节点转换为字典类型，若为空则返回空字典
        return options?.ToObject<Dictionary<string, List<string>>>() ?? [];
    }

    /// <summary>
    /// 获取指定配置项的值。
    /// </summary>
    /// <param name="key">配置项的名称。</param>
    /// <returns>指定配置项的值列表。如果配置项不存在，则返回空列表。</returns>
    public List<string> GetOption(string key)
    {
        // 加载所有配置项
        var options = LoadOptions();

        // 查找指定键对应的配置值，若不存在则返回空列表
        return options.TryGetValue(key, out var option) ? option : [];
    }

    /// <summary>
    /// 更新指定配置项的值。
    /// 如果配置项存在，则覆盖其值；否则新增配置项。
    /// </summary>
    /// <param name="key">配置项的名称。</param>
    /// <param name="newValues">配置项的新值列表。</param>
    public void UpdateOption(string key, List<string> newValues)
    {
        // 读取配置文件的内容并解析为 JSON 对象
        var jsonContent = File.ReadAllText(_configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        // 定位到 "Options" 节点，并更新指定键的值
        jsonObject["Options"]![key] = JArray.FromObject(newValues);

        // 将更新后的 JSON 对象写回配置文件
        File.WriteAllText(_configFilePath, jsonObject.ToString(Formatting.Indented));
    }

    /// <summary>
    /// 添加新的配置项。
    /// 如果配置项已存在，则不进行任何操作。
    /// </summary>
    /// <param name="key">新配置项的名称。</param>
    /// <param name="values">新配置项的值列表。</param>
    public void AddOption(string key, List<string> values)
    {
        // 读取配置文件的内容并解析为 JSON 对象
        var jsonContent = File.ReadAllText(_configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        // 检查 "Options" 节点下是否已存在指定键
        if (jsonObject["Options"]![key] != null) return;

        // 添加新的键值对到 "Options" 节点
        jsonObject["Options"]![key] = JArray.FromObject(values);

        // 将更新后的 JSON 对象写回配置文件
        File.WriteAllText(_configFilePath, jsonObject.ToString(Formatting.Indented));
    }
}