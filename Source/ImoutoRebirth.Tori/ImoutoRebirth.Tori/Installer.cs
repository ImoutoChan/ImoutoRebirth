using ImoutoRebirth.Tori.Services;
using Microsoft.Extensions.Logging;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace ImoutoRebirth.Tori;

public interface IInstaller
{
    void EasyInstallOrUpdate(DirectoryInfo updaterLocation, bool forceUpdate = false);
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

    public void EasyInstallOrUpdate(DirectoryInfo updaterLocation, bool forceUpdate)
    {
        _logger.LogInformation("Installing ImoutoRebirth app family");
        var isInstalled = _registryService.IsInstalled(out var installLocation);

        if (isInstalled)
        {
            _logger.LogInformation("Local version was found, updating");
            EasyUpdate(installLocation!, updaterLocation, forceUpdate);
        }
        else
        {
            _logger.LogInformation("No local version was found, installing from scratch");
            EasyInstall(updaterLocation);
        }
    }

    private void EasyUpdate(DirectoryInfo installLocation, DirectoryInfo updaterLocation, bool forceUpdate)
    {
        var newVersion = _versionService.GetNewVersion();
        var localVersion = _versionService.GetLocalVersion(installLocation);
        _logger.LogInformation("New version {NewVersion}", newVersion);
        
        if (localVersion == newVersion && !forceUpdate)
        {
            _logger.LogInformation("Everything is up to date");
        }
        else
        {
            _logger.LogInformation("Detected installed version {LocalVersion}, updating", localVersion);
            EasyUpdateProgram(newVersion, installLocation, updaterLocation);
        }
    }

    private void EasyUpdateProgram(string newVersion, DirectoryInfo installLocation, DirectoryInfo updaterLocation)
    {
        _configurationService.ActualizeConfigurationForUpdate(newVersion, installLocation, updaterLocation);

        _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        
        _versionService.SetLocalVersionAsNew(installLocation);

        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);
        
        _logger.LogInformation("ImoutoRebirth updated");
    }

    private void EasyInstall(DirectoryInfo updaterLocation)
    {
        var newVersion = _versionService.GetNewVersion();
        
        var installLocation = _configurationService.ActualizeConfigurationForInstall(newVersion, updaterLocation);
        _windowsServiceUpdater.UpdateService(installLocation, updaterLocation);
        _versionService.SetLocalVersionAsNew(installLocation);
        _configurationService.SaveActualConfigurationInNewServices(installLocation, updaterLocation);
        _registryService.SetInstalled(installLocation);

        _logger.LogInformation("ImoutoRebirth was installed");
    }
}
