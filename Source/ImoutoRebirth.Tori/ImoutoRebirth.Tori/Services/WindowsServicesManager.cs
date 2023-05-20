using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.Services;

public record WindowsService(string Name, string Status);

public interface IWindowsServicesManager
{
    void LogServices();

    void StopServices();

    void DeleteServices();

    void CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices);

    void StartServices();
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class FakeWindowsServicesManager : IWindowsServicesManager
{
    private readonly ILogger<WindowsServiceUpdater> _logger;

    public FakeWindowsServicesManager(ILogger<WindowsServiceUpdater> logger) => _logger = logger;
    
    public void LogServices()
    {
        var installedServicesString = GetServices().JoinStrings(x => $"({x.Name} {x.Status})", ", ");
        _logger.LogInformation("Installed services: {InstalledServices}", installedServicesString);
    }

    public void StopServices()
    {
        var services = GetWindowsServices().ToList();
        
        foreach (var service in services)
        {
            _logger.LogInformation("Stopping {ServiceName}", service.ServiceName);
        }

        foreach (var service in services)
        {
            _logger.LogInformation("Stopped {ServiceName}", service.ServiceName);
        }
    }

    public void DeleteServices()
    {
        var services = GetWindowsServices().ToList();

        foreach (var service in services)
        {
            _logger.LogInformation("Deleting {ServiceName}", service.ServiceName);
            _logger.LogInformation("RUN sc delete {ServiceName}", service.ServiceName);
            _logger.LogInformation("Deleted {ServiceName}", service.ServiceName);
        }
    }

    public void CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices)
    {
        foreach (var service in newServices)
        {
            _logger.LogInformation("Creating {ServiceName} with exe {ServiceExe}", service.Name, service.ExePath);
            _logger.LogInformation("RUN sc create {ServiceName} binPath= \"{ServiceExe}\"", service.Name, service.ExePath);
            _logger.LogInformation("Created {ServiceName}", service.Name);
        }
    }

    public void StartServices()
    {
        var services = GetWindowsServices().ToList();
        
        foreach (var service in services)
        {
            _logger.LogInformation("Starting {ServiceName}", service.ServiceName);
        }
    }

    private static IEnumerable<ServiceController> GetWindowsServices()
        => ServiceController.GetServices()
            .Where(x => x.ServiceName.StartsWith("ImoutoRebirth"));
    
    private static IEnumerable<WindowsService> GetServices() 
        => GetWindowsServices().Select(x => new WindowsService(x.ServiceName, x.Status.ToString()));
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class WindowsServicesManager : IWindowsServicesManager
{
    private readonly ILogger<WindowsServiceUpdater> _logger;

    public WindowsServicesManager(ILogger<WindowsServiceUpdater> logger) => _logger = logger;

    public void LogServices()
    {
        var installedServicesString = GetServices().JoinStrings(x => $"({x.Name} {x.Status})", ", ");
        _logger.LogInformation("Installed services: {InstalledServices}", installedServicesString);
    }

    public void StopServices()
    {
        var services = GetWindowsServices().ToList();
        
        foreach (var service in services.Where(x => x.Status != ServiceControllerStatus.Stopped))
        {
            _logger.LogInformation("Stopping {ServiceName}", service.ServiceName);
            service.Stop();
        }

        foreach (var service in services.Where(x => x.Status == ServiceControllerStatus.StopPending))
        {
            service.WaitForStatus(ServiceControllerStatus.Stopped);
            _logger.LogInformation("Stopped {ServiceName}", service.ServiceName);
        }
    }

    public void StartServices()
    {
        var services = GetWindowsServices().ToList();
        
        foreach (var service in services)
        {
            _logger.LogInformation("Starting {ServiceName}", service.ServiceName);
            service.Start();
        }
    }

    public void DeleteServices()
    {
        var services = GetWindowsServices().ToList();

        foreach (var service in services)
        {
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                _logger.LogCritical("Unable to delete running service {ServiceName}", service.ServiceName);
                throw new Exception($"Unable to delete running service {service.ServiceName}");
            }

            _logger.LogInformation("Deleting {ServiceName}", service.ServiceName);
            using var process = new Process();
            process.StartInfo.FileName = "sc";
            process.StartInfo.Arguments = $"delete {service.ServiceName}";
            process.Start();
            process.WaitForExit();
            _logger.LogInformation("Deleted {ServiceName}", service.ServiceName);
        }
    }

    public void CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices)
    {
        //sc.exe create <new_service_name> binPath= "<path_to_the_service_executable>"
        
        var existingServices = GetWindowsServices().ToList();

        var nameConflictService = existingServices.FirstOrDefault(x => newServices.Any(y => y.Name == x.ServiceName));
        if (nameConflictService != null)
        {
            _logger.LogCritical("Service with name {ServiceName} already exists", nameConflictService.ServiceName);
            throw new Exception($"Service with name {nameConflictService.ServiceName} already exists");
        }
        
        foreach (var service in newServices)
        {
            _logger.LogInformation("Creating {ServiceName} with exe {ServiceExe}", service.Name, service.ExePath);
            using var process = new Process();
            process.StartInfo.FileName = "sc";
            process.StartInfo.Arguments = $"create {service.Name} start= delayed-auto binPath= \"{service.ExePath}\"";
            process.Start();
            process.WaitForExit();
            _logger.LogInformation("Created {ServiceName}", service.Name);
        }
    }

    private static IEnumerable<ServiceController> GetWindowsServices()
        => ServiceController.GetServices()
            .Where(x => x.ServiceName.StartsWith("ImoutoRebirth"));
    
    private static IEnumerable<WindowsService> GetServices() 
        => GetWindowsServices().Select(x => new WindowsService(x.ServiceName, x.Status.ToString()));
}
