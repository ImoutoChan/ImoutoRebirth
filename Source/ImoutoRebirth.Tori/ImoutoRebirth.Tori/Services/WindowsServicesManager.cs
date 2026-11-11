using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Tori.Services;

public record WindowsService(string Name, string Status);

public interface IWindowsServicesManager
{
    Task LogServices();

    Task StopServices();

    Task DeleteServices();

    Task CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices);

    Task StartServices();
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class FakeWindowsServicesManager : IWindowsServicesManager
{
    private readonly ILogger<WindowsServiceUpdater> _logger;

    public FakeWindowsServicesManager(ILogger<WindowsServiceUpdater> logger) => _logger = logger;
    
    public async Task LogServices()
    {
        var installedServicesString = (await GetServices()).JoinStrings(x => $"({x.Name} {x.Status})", ", ");
        _logger.LogInformation("Installed services: {InstalledServices}", installedServicesString);
    }

    public async Task StopServices()
    {
        var services = await GetWindowsServices();
        
        foreach (var service in services)
        {
            _logger.LogInformation("Stopping {ServiceName}", service.ServiceName);
        }

        foreach (var service in services)
        {
            _logger.LogInformation("Stopped {ServiceName}", service.ServiceName);
        }
    }

    public async Task DeleteServices()
    {
        var services = await GetWindowsServices();

        foreach (var service in services)
        {
            _logger.LogInformation("Deleting {ServiceName}", service.ServiceName);
            _logger.LogInformation("RUN sc delete {ServiceName}", service.ServiceName);
            _logger.LogInformation("Deleted {ServiceName}", service.ServiceName);
        }
    }

    public Task CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices)
    {
        foreach (var service in newServices)
        {
            _logger.LogInformation("Creating {ServiceName} with exe {ServiceExe}", service.Name, service.ExePath);
            _logger.LogInformation("RUN sc create {ServiceName} binPath= \"{ServiceExe}\"", service.Name, service.ExePath);
            _logger.LogInformation("Created {ServiceName}", service.Name);
        }

        return Task.CompletedTask;
    }

    public async Task StartServices()
    {
        var services = await GetWindowsServices();
        
        foreach (var service in services)
        {
            _logger.LogInformation("Starting {ServiceName}", service.ServiceName);
        }
    }

    private static async Task<IReadOnlyCollection<ServiceController>> GetWindowsServices()
        => await Task.Run(() => ServiceController.GetServices()
            .Where(x => x.ServiceName.StartsWith("ImoutoRebirth"))
            .ToList());

    private static async Task<IReadOnlyCollection<WindowsService>> GetServices()
        => (await GetWindowsServices())
            .Select(x => new WindowsService(x.ServiceName, x.Status.ToString()))
            .ToList();
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class WindowsServicesManager : IWindowsServicesManager
{
    private const int ErrorServiceRequestTimeout = 1053;
    private const int ErrorServiceAlreadyRunning = 1056;
    private const int MaxStartAttempts = 10;
    private static readonly TimeSpan StartRetryDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PendingStatusTimeout = TimeSpan.FromMinutes(2);

    private readonly ILogger<WindowsServiceUpdater> _logger;

    public WindowsServicesManager(ILogger<WindowsServiceUpdater> logger) => _logger = logger;

    public async Task LogServices()
    {
        var installedServicesString
            = (await GetServices()).JoinStrings(x => $"({x.Name} {x.Status})", ", ");

        _logger.LogInformation("Installed services: {InstalledServices}", installedServicesString);
    }

    public async Task StopServices()
    {
        var services = await GetWindowsServices();

        var runningServices = services.Where(x => x.Status != ServiceControllerStatus.Stopped).ToList();
        
        foreach (var service in runningServices)
        {
            _logger.LogInformation("Stopping {ServiceName}", service.ServiceName);
            await Task.Run(() => service.Stop());
        }

        foreach (var service in runningServices)
        {
            await Task.Run(() => service.WaitForStatus(ServiceControllerStatus.Stopped));
            _logger.LogInformation("Stopped {ServiceName}", service.ServiceName);
        }
    }

    public async Task StartServices()
    {
        var services = await GetWindowsServices();
        
        foreach (var service in services)
        {
            await StartService(service);
            _logger.LogInformation("Started {ServiceName}", service.ServiceName);
        }
    }

    private async Task StartService(ServiceController service)
    {
        for (var attempt = 1; ; attempt++)
        {
            var status = await WaitForStableStatus(service);
            if (status == ServiceControllerStatus.Running)
                return;

            if (status != ServiceControllerStatus.Stopped)
                throw new InvalidOperationException($"Unable to start {service.ServiceName} in state {status}.");

            _logger.LogInformation("Starting {ServiceName} (attempt {Attempt}/{MaxAttempts})",
                service.ServiceName, attempt, MaxStartAttempts);
            try
            {
                await Task.Run(service.Start);
            }
            catch (InvalidOperationException ex) when (
                ex.InnerException is Win32Exception { NativeErrorCode: ErrorServiceRequestTimeout })
            {
                status = await WaitForStableStatus(service);
                if (status == ServiceControllerStatus.Running)
                    return;

                if (status != ServiceControllerStatus.Stopped || attempt == MaxStartAttempts)
                    throw;

                _logger.LogWarning("Windows timed out starting {ServiceName} (1053). Retrying in {Delay}.",
                    service.ServiceName, StartRetryDelay);
                await Task.Delay(StartRetryDelay);
                continue;
            }
            catch (InvalidOperationException ex) when (
                ex.InnerException is Win32Exception { NativeErrorCode: ErrorServiceAlreadyRunning })
            {
                // Another caller started the service between the status check and Start.
            }

            status = await WaitForStableStatus(service);
            if (status != ServiceControllerStatus.Running)
                throw new InvalidOperationException($"{service.ServiceName} entered state {status} during startup.");

            return;
        }
    }

    private static async Task<ServiceControllerStatus> WaitForStableStatus(ServiceController service)
    {
        var timer = Stopwatch.StartNew();
        while (true)
        {
            service.Refresh();
            var status = service.Status;
            if (status is not (ServiceControllerStatus.StartPending or ServiceControllerStatus.StopPending))
                return status;

            if (timer.Elapsed >= PendingStatusTimeout)
                throw new System.TimeoutException($"{service.ServiceName} remained {status} for {PendingStatusTimeout}.");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    public async Task DeleteServices()
    {
        var services = await GetWindowsServices();

        foreach (var service in services)
        {
            if (service.Status != ServiceControllerStatus.Stopped)
            {
                _logger.LogCritical("Unable to delete running service {ServiceName}", service.ServiceName);
                throw new Exception($"Unable to delete running service {service.ServiceName}");
            }

            _logger.LogInformation("Deleting {ServiceName}", service.ServiceName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"delete {service.ServiceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            await process!.WaitForExitAsync();
            _logger.LogInformation("Deleted {ServiceName}", service.ServiceName);
        }
    }

    public async Task CreateServices(IReadOnlyCollection<(string Name, string ExePath)> newServices)
    {
        if (newServices.Count == 0)
            return;

        //sc.exe create <new_service_name> binPath= "<path_to_the_service_executable>"
        var existingServices = await GetWindowsServices();

        var nameConflictService = existingServices.FirstOrDefault(x => newServices.Any(y => y.Name == x.ServiceName));
        if (nameConflictService != null)
        {
            _logger.LogCritical("Service with name {ServiceName} already exists", nameConflictService.ServiceName);
            throw new Exception($"Service with name {nameConflictService.ServiceName} already exists");
        }

        foreach (var service in newServices)
        {
            _logger.LogInformation("Creating {ServiceName} with exe {ServiceExe}", service.Name, service.ExePath);
            var startInfo = new ProcessStartInfo
            {
                FileName = "sc",
                Arguments = $"create {service.Name} start= delayed-auto binPath= \"{service.ExePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            await process!.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"Unable to create service {service.Name}: sc.exe exited with code {process.ExitCode}. {output} {error}");
            }

            _logger.LogInformation("Created {ServiceName}", service.Name);
        }
    }

    private static async Task<IReadOnlyCollection<ServiceController>> GetWindowsServices()
        => await Task.Run(() => ServiceController.GetServices()
            .Where(x => x.ServiceName.StartsWith("ImoutoRebirth"))
            .ToList());
    
    private static async Task<IReadOnlyCollection<WindowsService>> GetServices()
        => (await GetWindowsServices())
            .Select(x => new WindowsService(x.ServiceName, x.Status.ToString()))
            .ToList();
}
