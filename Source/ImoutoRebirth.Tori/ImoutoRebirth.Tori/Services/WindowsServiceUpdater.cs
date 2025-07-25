﻿using System.Diagnostics;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.Services;

public interface IWindowsServiceUpdater
{
    Task UpdateService(DirectoryInfo installLocation, DirectoryInfo updaterLocation);
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
        "ImoutoRebirth.Lamia",
    };
    
    private static readonly IReadOnlyCollection<(string ServiceName, string ShortcutName)> CreateShortcuts = new[]
    {
        ("ImoutoRebirth.Navigator", "ImoutoRebirth Navigator")
    };
    
    private static readonly IReadOnlyCollection<string> StopApplicationNames = new[]
    {
        "ImoutoRebirth.Navigator",
        "ImoutoViewer"
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

    public async Task UpdateService(DirectoryInfo installLocation, DirectoryInfo updaterLocation)
    {
        await _windowsServicesManager.LogServices();
        await _windowsServicesManager.StopServices();
        StopApplications();
        await _windowsServicesManager.DeleteServices();

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
                DeleteWithRetries(oldServiceDirectory);

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

            if (CreateShortcuts.Any(x => x.ServiceName == serviceName))
            {
                var exeName = oldServiceDirectory.GetFiles("*", SearchOption.AllDirectories)
                    .First(x => x.Extension == ".exe" && x.Name.Contains(serviceName)).FullName;

                _shortcutService.CreateShortcutToDesktop(
                    exeName,
                    CreateShortcuts.First(x => x.ServiceName == serviceName).ShortcutName);
            }
        }
        
        await _windowsServicesManager.CreateServices(windowsServicesToCreate);
        await _windowsServicesManager.StartServices();
    }

    private static void StopApplications()
    {
        foreach (var process in Process.GetProcesses())
        {
            if (!StopApplicationNames.Contains(process.ProcessName)) 
                continue;
            
            process.CloseMainWindow();
            if (!process.WaitForExit(3000))
                process.Kill();
        }
    }

    private static void DeleteWithRetries(DirectoryInfo oldServiceDirectory, int tryNumber = 0)
    {
        try
        {
            oldServiceDirectory.Delete(true);
        }
        catch
        {
            if (tryNumber > 3)
                throw;
            
            Thread.Sleep(TimeSpan.FromSeconds(tryNumber * 5 + 1));
            DeleteWithRetries(oldServiceDirectory, tryNumber + 1);
        }
    }
}
