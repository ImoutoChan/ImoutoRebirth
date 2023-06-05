using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.Services;

public interface IWindowsServiceUpdater
{
    void UpdateService(DirectoryInfo installLocation, DirectoryInfo updaterLocation);
}

public class WindowsServiceUpdater : IWindowsServiceUpdater
{
    private static readonly IReadOnlyCollection<string> WindowsServiceNames = new[]
    {
        "ImoutoRebirth.Arachne",
        "ImoutoRebirth.Harpy",
        "ImoutoRebirth.Kekkai",
        "ImoutoRebirth.Lilin",
        "ImoutoRebirth.Meido",
        "ImoutoRebirth.Room",
    };
    
    private static readonly IReadOnlyCollection<string> CreateShortcuts = new[]
    {
        "ImoutoRebirth.Navigator",
    };

    private readonly IWindowsServicesManager _windowsServicesManager;
    private readonly IShortcutService _shortcutService;
    private readonly ILogger<WindowsServiceUpdater> _logger;

    public WindowsServiceUpdater(
        IWindowsServicesManager windowsServicesManager,
        ILogger<WindowsServiceUpdater> logger,
        IShortcutService shortcutService)
    {
        _windowsServicesManager = windowsServicesManager;
        _logger = logger;
        _shortcutService = shortcutService;
    }

    public void UpdateService(DirectoryInfo installLocation, DirectoryInfo updaterLocation)
    {
        _windowsServicesManager.LogServices();
        _windowsServicesManager.StopServices();
        _windowsServicesManager.DeleteServices();

        var serviceDirectories = updaterLocation.GetDirectories();
        var windowsServicesToCreate = new List<(string Name, string ExePath)>();

        foreach (var newServiceDirectory in serviceDirectories)
        {
            var serviceName = newServiceDirectory.Name;
            
            if (newServiceDirectory.Name == "ImoutoRebirth.Tori")
                continue;
            
            var oldServiceDirectory = installLocation.CombineToDirectory(serviceName
                .Replace("ImoutoRebirth.", "")
                .Replace("Imouto.", ""));
            
            _logger.LogInformation("{ServiceName} | Cleaning existing service directory", serviceName);            
            if (oldServiceDirectory.Exists) 
                oldServiceDirectory.Delete(true);

            oldServiceDirectory.Create();
            
            _logger.LogInformation("{ServiceName} | Creating new directories", serviceName);             
            foreach (var dir in newServiceDirectory.GetDirectories("*", SearchOption.AllDirectories))
            {
                var subDirectoryToCreate = Path.Join(
                    oldServiceDirectory.FullName,
                    dir.FullName[(newServiceDirectory.FullName.Length + 1)..]);

                Directory.CreateDirectory(subDirectoryToCreate);
            }

            _logger.LogInformation("{ServiceName} | Copying new files", serviceName);
            foreach (var file in newServiceDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var fileToCreate = Path.Join(
                    oldServiceDirectory.FullName,
                    file.FullName[(newServiceDirectory.FullName.Length + 1)..]);
                
                file.CopyTo(fileToCreate, overwrite: true);
            }

            if (WindowsServiceNames.Contains(serviceName))
            {
                var exeName = oldServiceDirectory.GetFiles("*", SearchOption.AllDirectories)
                    .First(x => x.Extension == ".exe" && x.Name.Contains(serviceName)).FullName;

                _logger.LogInformation(
                    "{ServiceName} | Preparing to create service with exe {ServiceExe}", 
                    serviceName,
                    exeName);
                windowsServicesToCreate.Add((serviceName, exeName));
            }

            if (CreateShortcuts.Contains(serviceName))
            {
                var exeName = oldServiceDirectory.GetFiles("*", SearchOption.AllDirectories)
                    .First(x => x.Extension == ".exe" && x.Name.Contains(serviceName)).FullName;
                
                _shortcutService.CreateShortcutToDesktop(exeName, serviceName);
            }
        }
        
        _windowsServicesManager.CreateServices(windowsServicesToCreate);
        _windowsServicesManager.StartServices();
    }
}
