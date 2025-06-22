using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IConfigurationStorage
{
    AppConfiguration CurrentConfiguration { get; }

    bool ShouldInstallPostgreSql { get; set; }

    bool ShouldInstallRuntimes { get; set; }

    void UpdateConfiguration(Func<AppConfiguration, AppConfiguration> updater);
}

public class ConfigurationStorage : IConfigurationStorage
{
    public ConfigurationStorage(
        IRegistryService registryService,
        IOptions<AppSettings> options,
        IConfigurationService configurationService)
    {
        if (registryService.IsInstalled(out var installLocation))
        {
            var configuration = configurationService.PrepareFinalConfigurationFileForUpdate(
                installLocation,
                options.Value.UpdaterLocation);

            CurrentConfiguration = configuration;
        }
        else
        {
            var configuration = configurationService.PrepareFinalConfigurationFileForInstall(
                options.Value.UpdaterLocation);

            CurrentConfiguration = configuration;
        }
    }

    public AppConfiguration CurrentConfiguration { get; private set; }

    public bool ShouldInstallPostgreSql { get; set; }

    public bool ShouldInstallRuntimes { get; set; }

    public void UpdateConfiguration(Func<AppConfiguration, AppConfiguration> updater)
        => CurrentConfiguration = updater(CurrentConfiguration);
}
