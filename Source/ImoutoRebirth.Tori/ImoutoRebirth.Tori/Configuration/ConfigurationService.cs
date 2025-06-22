using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.Configuration;

public interface IConfigurationService
{
    AppConfiguration PrepareFinalConfigurationFileForUpdate(
        DirectoryInfo installLocation,
        DirectoryInfo updaterLocation);

    void SaveActualConfigurationInNewServices(DirectoryInfo installLocation, DirectoryInfo updaterLocation);

    AppConfiguration PrepareFinalConfigurationFileForInstall(DirectoryInfo updaterLocation);

    DirectoryInfo ActualizeFinalConfigurationFile(
        string newVersion,
        DirectoryInfo updaterLocation,
        AppConfiguration configuration);
}

public class ConfigurationService : IConfigurationService
{
    public const string ConfigurationFilename = "configuration.json";
    public const string ConfigurationFinalFilename = "configuration.final.json";

    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger) => _logger = logger;

    public AppConfiguration PrepareFinalConfigurationFileForUpdate(
        DirectoryInfo installLocation,
        DirectoryInfo updaterLocation)
    {
        _logger.LogInformation("Looking for configuration files");
        var fileInInstallLocation = installLocation.GetFiles().FirstOrDefault(x => x.Name == ConfigurationFilename);
        var fileInUpdaterLocation = updaterLocation.GetFiles().First(x => x.Name == ConfigurationFilename);

        var finalFilePath = updaterLocation.CombineToFilePath(ConfigurationFinalFilename);
        var fileToUse = fileInInstallLocation is { Exists: true }
            ? fileInInstallLocation
            : fileInUpdaterLocation;

        _logger.LogInformation("Using as final configuration file \"{ConfigurationFileSource}\"", fileToUse.FullName);
        var finalFile = fileToUse.CopyTo(finalFilePath, true);

        return AppConfiguration.ReadFromFile(finalFile);
    }


    public AppConfiguration PrepareFinalConfigurationFileForInstall(DirectoryInfo updaterLocation)
    {
        _logger.LogInformation("Looking for configuration files");
        var fileInUpdaterLocation = updaterLocation.GetFiles().First(x => x.Name == ConfigurationFilename);

        var finalFilePath = updaterLocation.CombineToFilePath(ConfigurationFinalFilename);
        var fileToUse = fileInUpdaterLocation;

        _logger.LogInformation("Using as final configuration file \"{ConfigurationFileSource}\"", fileToUse.FullName);
        var finalFile = fileToUse.CopyTo(finalFilePath, true);
        return AppConfiguration.ReadFromFile(finalFile);
    }

    public DirectoryInfo ActualizeFinalConfigurationFile(
        string newVersion,
        DirectoryInfo updaterLocation,
        AppConfiguration configuration)
    {
        var config = new ConfigurationBuilder(configuration);

        _logger.LogInformation("Writing built appsettings.Production.json files to services");
        config.WriteProductionConfigurations(newVersion, updaterLocation);

        return config.GetInstallLocation();
    }

    public void SaveActualConfigurationInNewServices(DirectoryInfo installLocation, DirectoryInfo updaterLocation)
    {
        var finalFileInUpdaterLocation = updaterLocation.CombineToFileInfo(ConfigurationFinalFilename);
        var configurationFileInInstallLocation = installLocation.CombineToFilePath(ConfigurationFilename);
        
        finalFileInUpdaterLocation.CopyTo(
            configurationFileInInstallLocation,
            overwrite: true);
    }
}
