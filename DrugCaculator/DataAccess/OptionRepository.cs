using DrugCalculator.Models;
using System;
using System.IO;
using System.Text.Json;

namespace DrugCalculator.DataAccess;

public class OptionRepository
{
    private static readonly Lazy<OptionRepository> _instance = new(() => new OptionRepository());
    private AppOptions _options;

    private OptionRepository()
    {
        LoadOptions();
    }

    public static OptionRepository Instance => _instance.Value;

    public void LoadOptions(string filePath = "AppOptions.json")
    {
        try
        {
            var json = File.ReadAllText(filePath);
            _options = JsonSerializer.Deserialize<AppOptions>(json);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load configuration: {ex.Message}");
        }
    }

    public string[] GetRouteOptions()
    {
        return _options?.Options?.RouteOptions ?? [];
    }

    public string[] GetUnitOptions()
    {
        return _options?.Options?.UnitOptions ?? [];
    }

    public string[] GetFrequencyOptions()
    {
        return _options?.Options?.FrequencyOptions ?? [];
    }

    public string[] GetLogLevelOptions()
    {
        return _options?.Options?.LogLevelOptions ?? [];
    }
}