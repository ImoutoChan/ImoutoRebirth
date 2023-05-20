﻿using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori;

public interface IConfigurationService
{
    void ActualizeConfigurationForUpdate(
        string newVersion,
        DirectoryInfo installLocation,
        DirectoryInfo updaterLocation);

    void SaveActualConfigurationInNewServices(DirectoryInfo installLocation, DirectoryInfo updaterLocation);

    DirectoryInfo ActualizeConfigurationForInstall(string newVersion, DirectoryInfo updaterLocation);
}

public class ConfigurationService : IConfigurationService
{
    private const string ConfigurationFilename = "configuration.json";
    private const string ConfigurationFinalFilename = "configuration.final.json";

    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(ILogger<ConfigurationService> logger) => _logger = logger;

    public void ActualizeConfigurationForUpdate(
        string newVersion,  
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

        var config = new ConfigurationBuilder(finalFile);

        _logger.LogInformation("Writing built appsettings.Production.json files to services");
        config.WriteProductionConfigurations(newVersion);
    }

    public DirectoryInfo ActualizeConfigurationForInstall(string newVersion, DirectoryInfo updaterLocation)
    {
        _logger.LogInformation("Looking for configuration files");
        var fileInUpdaterLocation = updaterLocation.GetFiles().First(x => x.Name == ConfigurationFilename);

        var finalFilePath = updaterLocation.CombineToFilePath(ConfigurationFinalFilename);
        var fileToUse = fileInUpdaterLocation;

        _logger.LogInformation("Using as final configuration file \"{ConfigurationFileSource}\"", fileToUse.FullName);
        var finalFile = fileToUse.CopyTo(finalFilePath, true);

        var config = new ConfigurationBuilder(finalFile);

        _logger.LogInformation("Writing built appsettings.Production.json files to services");
        config.WriteProductionConfigurations(newVersion);

        return new DirectoryInfo(config.GetInstallLocation());
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
