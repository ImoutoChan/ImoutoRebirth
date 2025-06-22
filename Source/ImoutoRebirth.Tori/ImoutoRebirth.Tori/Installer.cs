using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.Logging;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace ImoutoRebirth.Tori;

public interface IInstaller
{
    Task EasyInstallOrUpdate(DirectoryInfo updaterLocation, bool forceUpdate = false);

    Task WizardInstallOrUpdate(DirectoryInfo updaterLocation, bool forceUpdate, AppConfiguration newConfiguration);
}

public class Installer : IInstaller
{
    private readonly IRegistryService _registryService;
    private readonly IVersionService _versionService;
    private readonly IConfigurationService _configurationService;
    private readonly IWindowsServiceUpdater _windowsServiceUpdater;
    private readonly ILogger<Installer> _logger;

    public Installer(
        IRegistryService registryService,
        IVersionService versionService,
        ILogger<Installer> logger,
        IConfigurationService configurationService,
        IWindowsServiceUpdater windowsServiceUpdater)
    {
        _registryService = registryService;
        _versionService = versionService;
        _logger = logger;
        _configurationService = configurationService;
        _windowsServiceUpdater = windowsServiceUpdater;
    }

    public async Task EasyInstallOrUpdate(DirectoryInfo updaterLocation, bool forceUpdate)
    {
        _logger.LogInformation("Installing ImoutoRebirth app family");
        var isInstalled = _registryService.IsInstalled(out var installLocation);

        if (isInstalled)
        {
            _logger.LogInformation("Local version was found, updating");
            await EasyUpdate(installLocation!, updaterLocation, forceUpdate);
        }
        else
        {
            _logger.LogInformation("No local version was found, installing from scratch");
            await EasyInstall(updaterLocation);
        }
    }

    public async Task WizardInstallOrUpdate(
        DirectoryInfo updaterLocation,
        bool forceUpdate,
        AppConfiguration newConfiguration)
    {
        _logger.LogInformation("Installing ImoutoRebirth app family");

        if (_registryService.IsInstalled(out var installLocation))
        {
            _logger.LogInformation("Local version was found, updating");
            await WizardUpdate(installLocation, updaterLocation, forceUpdate, newConfiguration);
        }
        else
        {
            _logger.LogInformation("No local version was found, installing from scratch");
            await WizardInstall(updaterLocation, newConfiguration);
        }
    }

    private async Task EasyUpdate(DirectoryInfo installLocation, DirectoryInfo updaterLocation, bool forceUpdate)
    {
        var newVersion = _versionService.GetNewVersion();
        var localVersion = await _versionService.GetLocalVersion(installLocation);
        _logger.LogInformation("New version {NewVersion}", newVersion);
        
        if (localVersion == newVersion && !forceUpdate)
        {
            _logger.LogInformation("Everything is up to date");
        }
        else
        {
            _logger.LogInformation("Detected installed version {LocalVersion}, updating", localVersion);
            await EasyUpdateProgram(newVersion, installLocation, updaterLocation);
        }
    }

    private async Task WizardUpdate(
        DirectoryInfo installLocation,
        DirectoryInfo updaterLocation,
        bool forceUpdate,
        AppConfiguration newConfiguration)
    {
        var newVersion = _versionService.GetNewVersion();
        var localVersion = await _versionService.GetLocalVersion(installLocation);
        _logger.LogInformation("New version {NewVersion}", newVersion);

        if (localVersion == newVersion && !forceUpdate)
        {
            _logger.LogInformation("Everything is up to date");
        }
        else
        {
            _logger.LogInformation("Detected installed version {LocalVersion}, updating", localVersion);
            await WizardUpdateProgram(newVersion, installLocation, updaterLocation, newConfiguration);
        }
    }

    private async Task EasyUpdateProgram(string newVersion, DirectoryInfo installLocation, DirectoryInfo updaterLocation)
    {
        var configuration = await _configurationService.PrepareFinalConfigurationFileForUpdate(installLocation, updaterLocation);
        await _configurationService.ActualizeFinalConfigurationFile(newVersion, updaterLocation, configuration);

        await _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        await _versionService.SetLocalVersionAsNew(installLocation);
        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);
        
        _logger.LogInformation("ImoutoRebirth updated");
    }

    private async Task WizardUpdateProgram(
        string newVersion,
        DirectoryInfo installLocation,
        DirectoryInfo updaterLocation,
        AppConfiguration newConfiguration)
    {
        await _configurationService.ActualizeFinalConfigurationFile(newVersion, updaterLocation, newConfiguration);

        await _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        await _versionService.SetLocalVersionAsNew(installLocation);
        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);

        _logger.LogInformation("ImoutoRebirth updated");
    }

    private async Task EasyInstall(DirectoryInfo updaterLocation)
    {
        var newVersion = _versionService.GetNewVersion();
        
        var configuration = await _configurationService.PrepareFinalConfigurationFileForInstall(updaterLocation);
        var installLocation = await _configurationService.ActualizeFinalConfigurationFile(newVersion, updaterLocation, configuration);

        await _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        await _versionService.SetLocalVersionAsNew(installLocation);
        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);
        _registryService.SetInstalled(installLocation);

        _logger.LogInformation("ImoutoRebirth was installed");
    }

    private async Task WizardInstall(DirectoryInfo updaterLocation, AppConfiguration newConfiguration)
    {
        var newVersion = _versionService.GetNewVersion();

        var installLocation = await _configurationService.ActualizeFinalConfigurationFile(
            newVersion,
            updaterLocation,
            newConfiguration);

        await _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        await _versionService.SetLocalVersionAsNew(installLocation);
        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);
        _registryService.SetInstalled(installLocation);

        _logger.LogInformation("ImoutoRebirth was installed");
    }
}
