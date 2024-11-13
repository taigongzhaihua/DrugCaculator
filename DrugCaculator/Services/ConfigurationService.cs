using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace DrugCalculator.Services;

public class ConfigurationService(string configFilePath = "AppOptions.json")
{
    // 读取配置文件
    public Dictionary<string, List<string>> LoadOptions()
    {
        var jsonContent = File.ReadAllText(configFilePath);
        var options = JObject.Parse(jsonContent)["Options"];
        return options?.ToObject<Dictionary<string, List<string>>>();
    }

    // 读取指定的配置项
    public List<string> GetOption(string key)
    {
        var options = LoadOptions();
        return options.TryGetValue(key, out var option) ? option : [];
    }

    // 修改指定的配置项
    public void UpdateOption(string key, List<string> newValues)
    {
        var jsonContent = File.ReadAllText(configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        jsonObject["Options"]![key] = JArray.FromObject(newValues);

        File.WriteAllText(configFilePath, jsonObject.ToString(Formatting.Indented));
    }

    // 添加新的配置项
    public void AddOption(string key, List<string> values)
    {
        var jsonContent = File.ReadAllText(configFilePath);
        var jsonObject = JObject.Parse(jsonContent);

        if (jsonObject["Options"]![key] != null) return;
        jsonObject["Options"]![key] = JArray.FromObject(values);
        File.WriteAllText(configFilePath, jsonObject.ToString(Formatting.Indented));
    }
}