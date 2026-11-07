using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.ServiceProcess;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

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
        if (newServices.Count != 0)
        {
            _logger.LogInformation(
                "Would ensure the system-wide Windows service startup timeout is at least {TimeoutSeconds} seconds; increasing it requires a Windows restart",
                WindowsServicesManager.MinimumStartupTimeoutMilliseconds / 1000);
        }

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
    internal const int MinimumStartupTimeoutMilliseconds = 120_000;
    private const string ServiceControlRegistryPath = @"SYSTEM\CurrentControlSet\Control";
    private const string StartupTimeoutRegistryValue = "ServicesPipeTimeout";

    private readonly ILogger<WindowsServiceUpdater> _logger;
    private TimeSpan _startupTimeout = TimeSpan.FromMilliseconds(MinimumStartupTimeoutMilliseconds);

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
            _logger.LogInformation("Starting {ServiceName}", service.ServiceName);
            await Task.Run(() =>
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, _startupTimeout);
            });
            _logger.LogInformation("Started {ServiceName}", service.ServiceName);
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

        ConfigureServiceStartupTimeout();

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

    private void ConfigureServiceStartupTimeout()
    {
        using var controlKey = Registry.LocalMachine.OpenSubKey(ServiceControlRegistryPath, writable: true)
            ?? throw new InvalidOperationException("Unable to open the Windows Service Control Manager settings.");

        var previousValue = controlKey.GetValue(StartupTimeoutRegistryValue);
        var timeoutMilliseconds = EnsureMinimumStartupTimeout(controlKey);
        _startupTimeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

        if (previousValue is int previousTimeout && unchecked((uint)previousTimeout) == timeoutMilliseconds)
        {
            _logger.LogInformation(
                "Windows service startup timeout is already {TimeoutSeconds} seconds",
                _startupTimeout.TotalSeconds);
            return;
        }

        // SCM reads this system-wide setting at boot; sc.exe create has no per-service equivalent.
        _logger.LogWarning(
            "Increased the system-wide Windows service startup timeout to {TimeoutSeconds} seconds. Restart Windows for the Service Control Manager to use the new timeout",
            _startupTimeout.TotalSeconds);
    }

    internal static uint EnsureMinimumStartupTimeout(RegistryKey controlKey)
    {
        // REG_DWORD is unsigned, although RegistryKey returns it as a signed Int32.
        if (controlKey.GetValue(StartupTimeoutRegistryValue) is int configuredTimeout)
        {
            var timeoutMilliseconds = unchecked((uint)configuredTimeout);
            if (timeoutMilliseconds >= MinimumStartupTimeoutMilliseconds)
                return timeoutMilliseconds;
        }

        controlKey.SetValue(
            StartupTimeoutRegistryValue,
            MinimumStartupTimeoutMilliseconds,
            RegistryValueKind.DWord);
        return MinimumStartupTimeoutMilliseconds;
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
