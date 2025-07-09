using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IConfigurationStorage
{
    AppConfiguration CurrentConfiguration { get; }

    bool ShouldInstallPostgreSql { get; set; }

    bool ShouldInstallRuntimes { get; set; }

    string NewVersion { get; }

    bool IsUpdating { get; }

    string CurrentVersion { get; }

    bool InstallLocationEditable { get; }

    string InstallLocation { get; }

    Task ConfigurationLoaded { get; }

    void UpdateConfiguration(Func<AppConfiguration, AppConfiguration> updater);

    PostgresConfiguration ExtractCurrentPostgresConfiguration();
}

public class ConfigurationStorage : IConfigurationStorage
{
    private readonly IRegistryService _registryService;
    private readonly IOptions<AppSettings> _options;
    private readonly IConfigurationService _configurationService;
    private readonly IVersionService _versionService;

    public ConfigurationStorage(
        IRegistryService registryService,
        IOptions<AppSettings> options,
        IConfigurationService configurationService,
        IVersionService versionService)
    {
        _registryService = registryService;
        _options = options;
        _configurationService = configurationService;
        _versionService = versionService;

        ConfigurationLoaded = Load();
    }

    public AppConfiguration CurrentConfiguration { get; private set; } = default!;

    public bool ShouldInstallPostgreSql { get; set; }

    public bool ShouldInstallRuntimes { get; set; }

    public string NewVersion { get; private set; } = "loading...";

    public bool IsUpdating { get; private set; }

    public string CurrentVersion { get; private set; } = "loading...";

    public bool InstallLocationEditable { get; private set; }

    public string InstallLocation { get; private set; } = "loading...";

    public Task ConfigurationLoaded { get; private set; }

    public void UpdateConfiguration(Func<AppConfiguration, AppConfiguration> updater)
        => CurrentConfiguration = updater(CurrentConfiguration);

    public PostgresConfiguration ExtractCurrentPostgresConfiguration()
    {
        var builder = new NpgsqlConnectionStringBuilder(CurrentConfiguration.Connection.RoomConnectionString);
        return new(builder.Host!, builder.Port, builder.Username!, builder.Password!);
    }

    private async Task Load()
    {
        if (_registryService.IsInstalled(out var installLocation))
        {
            var configuration = await _configurationService
                .PrepareFinalConfigurationFileForUpdate(installLocation, _options.Value.UpdaterLocation);

            CurrentConfiguration = configuration;
            CurrentVersion = await _versionService.GetLocalVersion(installLocation);
            IsUpdating = true;
            InstallLocation = installLocation.FullName;
            InstallLocationEditable = false;
        }
        else
        {
            var configuration = await _configurationService
                .PrepareFinalConfigurationFileForInstall(_options.Value.UpdaterLocation);

            CurrentConfiguration = configuration;
            CurrentVersion = "not found";
            IsUpdating = false;
            InstallLocation = configuration.InstallLocation;
            InstallLocationEditable = true;
        }

        NewVersion = _versionService.GetNewVersion();
    }
}

public record PostgresConfiguration(string Host, int Port, string User, string Pass);
