using System;
using System.IO;
using System.Text.Json;

namespace Pomidoras.Models.Timer.Configuration.Repository;

public class JsonTimerConfigurationRepository : ITimerConfigurationRepository
{
    private static readonly string ConfigurationFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Pomidoras",
        "config.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public TimerConfiguration? GetConfiguration()
    {
        if (!File.Exists(ConfigurationFilePath))
        {
            return null;
        }

        string jsonContent = File.ReadAllText(ConfigurationFilePath);
        return JsonSerializer.Deserialize<TimerConfiguration>(jsonContent, Options);
    }

    public TimerConfiguration SaveConfiguration(TimerConfiguration configuration)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationFilePath)!);
        string jsonContent = JsonSerializer.Serialize(configuration, Options);
        File.WriteAllText(ConfigurationFilePath, jsonContent);
        
        return configuration;
    }
}
