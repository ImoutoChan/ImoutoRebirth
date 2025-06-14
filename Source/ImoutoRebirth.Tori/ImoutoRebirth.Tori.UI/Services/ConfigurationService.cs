using System.IO;
using System.Text.Json;
using ImoutoRebirth.Tori.UI.Models;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IConfigurationService
{
    Task<Configuration?> LoadExistingConfigurationAsync(DirectoryInfo installLocation);
    Task SaveConfigurationAsync(Configuration configuration, DirectoryInfo installLocation);
}

public class ConfigurationService : IConfigurationService
{
    private const string ConfigFileName = "configuration.json";

    public async Task<Configuration?> LoadExistingConfigurationAsync(DirectoryInfo installLocation)
    {
        var configFilePath = Path.Combine(installLocation.FullName, ConfigFileName);
        if (!File.Exists(configFilePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(configFilePath);
            return JsonSerializer.Deserialize<Configuration>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveConfigurationAsync(Configuration configuration, DirectoryInfo installLocation)
    {
        if (!installLocation.Exists)
            installLocation.Create();

        var configFilePath = Path.Combine(installLocation.FullName, ConfigFileName);
        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(configFilePath, json);
    }
} 