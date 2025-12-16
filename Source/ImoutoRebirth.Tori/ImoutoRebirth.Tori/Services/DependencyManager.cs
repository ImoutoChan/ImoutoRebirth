using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Tori.Services;

public interface IDependencyManager
{
    IReadOnlyCollection<InstalledPostgresInfo> GetPostgresWindowsServices();

    bool IsPostgresPortInUse();

    bool IsPostgresInstalled();

    Task<bool> IsDotnetAspNetRuntimeInstalled(string version);

    Task<bool> IsDotnetDesktopRuntimeInstalled(string version);

    Task<IReadOnlyCollection<string>> GetDotnetRuntimes();

    Task<bool> InstallPostgres(int port, string pass);

    Task<bool> InstallDotnetAspNetRuntime(string version);

    Task<bool> InstallDotnetDesktopRuntime(string version);
}

public record InstalledPostgresInfo(string? ServiceName, Version? Version);

public class DependencyManagerOptions
{
    public Action<string>? ProcessConsoleOutput { get; set; }

    public bool IsDryRun { get; init; }
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public partial class DependencyManager : IDependencyManager
{
    private const string DefaultPostgresVersion = "16.8.0";
    private const int DefaultPostgresPort = 5432;
    private const string DefaultPostgresPassword = "postgres";

    private readonly bool _isDryRun;

    private readonly ILogger<DependencyManager> _logger;
    private readonly IOptions<DependencyManagerOptions> _options;
    private readonly Lazy<Task<ExecuteCommandResult>> _dotnetRuntimesOutput;
    private readonly Lazy<Task<bool>> _ensureChocoInstalled;

    public DependencyManager(ILogger<DependencyManager> logger, IOptions<DependencyManagerOptions> options)
    {
        _logger = logger;
        _options = options;
        _dotnetRuntimesOutput = new(() => ExecuteDotnetCommand("--list-runtimes"), LazyThreadSafetyMode.ExecutionAndPublication);
        _ensureChocoInstalled = new(() => EnsureChocoInstalled(), LazyThreadSafetyMode.ExecutionAndPublication);

        _isDryRun = options.Value.IsDryRun;
    }

    public IReadOnlyCollection<InstalledPostgresInfo> GetPostgresWindowsServices()
    {
        try
        {
            var windowsServices = ServiceController.GetServices();
            return windowsServices
                .Where(x => x.ServiceName.StartsWithIgnoreCase("postgresql"))
                .Select(x => new InstalledPostgresInfo(x.ServiceName, ExtractPostgresVersion(x.ServiceName)))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get postgres windows services");
            return [];
        }
    }

    public bool IsPostgresPortInUse()
    {
        try
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnections = ipGlobalProperties.GetActiveTcpListeners();

            return tcpConnections.Any(x => x.Port == DefaultPostgresPort);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check if postgres port is in use");
            return false;
        }
    }

    public bool IsPostgresInstalled()
    {
        var postgresServices = GetPostgresWindowsServices();
        if (postgresServices.Any())
            return true;

        return IsPostgresPortInUse();
    }

    public async Task<bool> IsDotnetAspNetRuntimeInstalled(string version)
    {
        var outputResult = await _dotnetRuntimesOutput.Value;

        var pattern = $@"Microsoft\.AspNetCore\.App\s+{Regex.Escape(version)}\b";
        return Regex.IsMatch(outputResult.Output, pattern);
    }

    public async Task<bool> IsDotnetDesktopRuntimeInstalled(string version)
    {
        var outputResult = await _dotnetRuntimesOutput.Value;

        var pattern = $@"Microsoft\.WindowsDesktop\.App\s+{Regex.Escape(version)}\b";
        return Regex.IsMatch(outputResult.Output, pattern);
    }

    public async Task<IReadOnlyCollection<string>> GetDotnetRuntimes()
    {
        var outputResult = await _dotnetRuntimesOutput.Value;
        return outputResult.Output.Split("\n");
    }

    public async Task<bool> InstallPostgres(int port = DefaultPostgresPort, string pass = DefaultPostgresPassword)
    {
        _logger.LogInformation("Installing PostgreSQL {Version}...", DefaultPostgresVersion);

        var arguments = $"install postgresql16 --version {DefaultPostgresVersion} -y --params \"'/Password:{pass} /Port:{port}'\"";
        var result = await ExecuteChocoCommand(arguments);

        if (result.ExitCode == 0)
        {
            _logger.LogInformation("PostgreSQL installed");
            return true;
        }
        else
        {
            _logger.LogWarning("PostgreSQL installation failed: {Error}", result.Error);
            return false;
        }
    }

    public async Task<bool> InstallDotnetAspNetRuntime(string version)
    {
        _logger.LogInformation("Installing ASP.NET Core runtime {Version}...", version);

        var result = await ExecuteChocoCommand($"install dotnet-aspnetruntime --version {version} -y");

        if (result.ExitCode == 0)
        {
            _logger.LogInformation("ASP.NET Core runtime installation completed");
            return true;
        }
        else
        {
            _logger.LogWarning("ASP.NET Core runtime installation failed: {Error}", result.Error);
            return false;
        }
    }

    public async Task<bool> InstallDotnetDesktopRuntime(string version)
    {
        _logger.LogInformation("Installing .NET Desktop runtime {Version}...", version);

        var result = await ExecuteChocoCommand($"install dotnet-desktopruntime --version {version} -y");

        if (result.ExitCode == 0)
        {
            _logger.LogInformation(".NET Desktop runtime installation completed");
            return true;
        }
        else
        {
            _logger.LogWarning(".NET Desktop runtime installation failed: {Error}", result.Error);
            return false;
        }
    }

    private async Task<ExecuteCommandResult> ExecuteDotnetCommand(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            LogProcessStartInfo(startInfo);

            using var process = Process.Start(startInfo);

            if (process == null)
                return new ExecuteCommandResult(-1, string.Empty, "Failed to start process");

            return await TraceProcessOutputAndWaitForExitWithResult(process);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute dotnet command: {Arguments}", arguments);
            return new ExecuteCommandResult(-1, "", ex.ToString());
        }
    }

    private async Task<ExecuteCommandResult> ExecuteChocoCommand(string arguments)
    {
        try
        {
            if (!await _ensureChocoInstalled.Value)
                return new ExecuteCommandResult(-1, "", "Chocolatey not installed");

            if (_isDryRun)
            {
                arguments = $"{arguments} --noop";
                _logger.LogInformation("Running in dry run mode, command will not make actual changes");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            LogProcessStartInfo(startInfo);

            using var process = Process.Start(startInfo);
            if (process == null)
                return new ExecuteCommandResult(-1, string.Empty, "Failed to start process");

            return await TraceProcessOutputAndWaitForExitWithResult(process);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute choco command: {Arguments}", arguments);
            return new ExecuteCommandResult(-1, "", ex.ToString());
        }
    }

    private void LogProcessStartInfo(ProcessStartInfo startInfo)
        => _logger.LogInformation("!! {Command} {Arguments}", startInfo.FileName, startInfo.Arguments);

    private async Task<ExecuteCommandResult> TraceProcessOutputAndWaitForExitWithResult(Process process)
    {
        var processConsoleOutput = _options.Value.ProcessConsoleOutput;

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        if (processConsoleOutput != null)
        {
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                    return;

                outputBuilder.AppendLine(e.Data);
                processConsoleOutput.Invoke(e.Data);
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null)
                    return;

                errorBuilder.AppendLine(e.Data);
                processConsoleOutput.Invoke(e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return new ExecuteCommandResult(process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
        }
        else
        {
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new ExecuteCommandResult(process.ExitCode, output, error);
        }
    }

    private async Task<bool> EnsureChocoInstalled()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-Command \"Get-Command choco -ErrorAction SilentlyContinue\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            LogProcessStartInfo(startInfo);

            using var process = Process.Start(startInfo);
            if (process == null)
                return false;

            var result = await TraceProcessOutputAndWaitForExitWithResult(process);

            if (result.ExitCode != 0)
            {
                if (_isDryRun)
                {
                    _logger.LogInformation(
                        "Chocolatey not found, installation of Chocolatey in dry run mode is impossible");
                    return true;
                }

                _logger.LogInformation("Chocolatey not found, installing...");

                var installStartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                LogProcessStartInfo(installStartInfo);

                using var installProcess = Process.Start(installStartInfo);
                if (installProcess == null)
                    return false;

                var chocoInstall = await TraceProcessOutputAndWaitForExitWithResult(installProcess);

                _logger.LogInformation(
                    "Chocolatey installation completed with exit code: {ExitCode}",
                    chocoInstall.ExitCode);

                UpdateEnvPath();

                return true;
            }
            else
            {
                _logger.LogInformation("Chocolatey is already installed");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check or install Chocolatey");
            return false;
        }
    }

    private void UpdateEnvPath()
    {
        const string chocoBinPath = @"C:\ProgramData\chocolatey\bin";

        var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
        if (currentPath.Split(';').ContainsIgnoreCase(chocoBinPath))
            return;

        var newPath = currentPath + ";" + chocoBinPath;
        Environment.SetEnvironmentVariable("PATH", newPath);
        _logger.LogInformation("Appended Chocolatey bin path to current process PATH");
    }

    private static Version? ExtractPostgresVersion(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        // Special case for "postgresql-x64-16" pattern
        var serviceNameMatch = Postgres16VersionRegex().Match(text);
        if (serviceNameMatch.Success)
        {
            var major = int.Parse(serviceNameMatch.Groups[1].Value);
            return new Version(major, 0, 0);
        }

        // Generic version pattern
        var match = PostgresVersionRegex().Match(text);
        if (!match.Success)
            return null;

        var vMajor = int.Parse(match.Groups[1].Value);
        var vMinor = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        var vBuild = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        return new Version(vMajor, vMinor, vBuild);
    }

    [GeneratedRegex(@"(\d+)(?:\.(\d+))?(?:\.(\d+))?")]
    private static partial Regex PostgresVersionRegex();

    [GeneratedRegex(@"postgresql-x64-(\d+)")]
    private static partial Regex Postgres16VersionRegex();
}

public record ExecuteCommandResult(int ExitCode, string Output, string Error);
