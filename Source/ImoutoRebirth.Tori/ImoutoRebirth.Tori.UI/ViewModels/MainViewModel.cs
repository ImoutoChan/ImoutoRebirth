using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ImoutoRebirth.Tori.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IRegistryService _registryService;
    private readonly IConfigurationService _configurationService;
    private DirectoryInfo? _existingInstallLocation;

    [ObservableProperty]
    private Configuration _configuration = new();

    [ObservableProperty]
    private string _statusMessage = "Checking system requirements...";

    [ObservableProperty]
    private bool _isInstalling;

    [ObservableProperty]
    private bool _hasDotnet;

    [ObservableProperty]
    private bool _hasRequiredRuntimes;

    [ObservableProperty]
    private bool _hasPostgres;

    [ObservableProperty]
    private string _postgresStatus = "Checking...";

    [ObservableProperty]
    private bool _isExistingInstallation;

    [ObservableProperty]
    private string? _existingInstallPath;
    
    [ObservableProperty]
    private string _installButtonText = "Install";
    
    [ObservableProperty]
    private bool _shouldInstallDotnetRuntime;
    
    [ObservableProperty]
    private bool _shouldInstallPostgres;

    private const string RequiredAspNetCoreVersion = "9.0.4";
    private const string RequiredDesktopVersion = "9.0.4";

    public MainViewModel()
    {
        _registryService = new RegistryService();
        _configurationService = new ConfigurationService();
        
        CheckForExistingInstallation();
        CheckSystemRequirements();
    }

    private async void CheckForExistingInstallation()
    {
        IsExistingInstallation = _registryService.IsInstalled(out _existingInstallLocation);

        if (IsExistingInstallation && _existingInstallLocation != null)
        {
            ExistingInstallPath = _existingInstallLocation.FullName;
            InstallButtonText = "Update";
            StatusMessage = $"Detected existing installation at {ExistingInstallPath}";

            var existingConfig = await _configurationService.LoadExistingConfigurationAsync(_existingInstallLocation);
            if (existingConfig != null)
            {
                Configuration = existingConfig;
                StatusMessage = $"Loaded configuration from existing installation at {ExistingInstallPath}";
            }
        }
    }

    private async void CheckSystemRequirements()
    {
        await CheckDotnetInstallation();
        await CheckPostgresInstallation();
    }

    private async Task CheckDotnetInstallation()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            HasDotnet = true;
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            HasRequiredRuntimes = output.Contains($"Microsoft.AspNetCore.App {RequiredAspNetCoreVersion}") &&
                                output.Contains($"Microsoft.WindowsDesktop.App {RequiredDesktopVersion}");

            StatusMessage = HasRequiredRuntimes
                ? "All required .NET runtimes are installed"
                : "Some required .NET runtimes are missing";
        }
        catch
        {
            HasDotnet = false;
            HasRequiredRuntimes = false;
            StatusMessage = ".NET SDK is not installed";
        }
    }

    private async Task CheckPostgresInstallation()
    {
        try
        {
            // Check if port 5432 is in use
            var netstatProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c netstat -ano | findstr :5431",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            netstatProcess.Start();
            var netstatOutput = await netstatProcess.StandardOutput.ReadToEndAsync();
            await netstatProcess.WaitForExitAsync();

            HasPostgres = !string.IsNullOrEmpty(netstatOutput) && netstatOutput.Contains("LISTENING");

            if (HasPostgres)
            {
                PostgresStatus = "PostgreSQL is running";
                return;
            }

            // If port is not in use, check services
            var servicesProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c sc query | findstr postgres",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            servicesProcess.Start();
            var servicesOutput = await servicesProcess.StandardOutput.ReadToEndAsync();
            await servicesProcess.WaitForExitAsync();

            if (!string.IsNullOrEmpty(servicesOutput))
            {
                HasPostgres = true;
                PostgresStatus = "PostgreSQL service is installed but not running";
            }
            else
            {
                PostgresStatus = "PostgreSQL is not installed";
            }
        }
        catch (Exception ex)
        {
            HasPostgres = false;
            PostgresStatus = $"Error checking PostgreSQL status: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task InstallDotnetRuntimes()
    {
        if (HasRequiredRuntimes)
            return;

        IsInstalling = true;
        StatusMessage = "Installing .NET runtimes...";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "choco",
                    Arguments = "install dotnet-9.0-runtime dotnet-9.0-aspnetcore-runtime -y",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    StatusMessage = e.Data;
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            await CheckDotnetInstallation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error installing .NET runtimes: {ex.Message}";
        }
        finally
        {
            IsInstalling = false;
        }
    }
    
    [RelayCommand]
    private async Task InstallPostgres()
    {
        if (HasPostgres)
            return;

        IsInstalling = true;
        StatusMessage = "Installing PostgreSQL...";

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "choco",
                    Arguments = "install postgresql -y",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    StatusMessage = e.Data;
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();

            await CheckPostgresInstallation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error installing PostgreSQL: {ex.Message}";
        }
        finally
        {
            IsInstalling = false;
        }
    }

    [RelayCommand]
    private async Task Install()
    {
        try
        {
            IsInstalling = true;
            
            // Install .NET Runtime if needed and selected
            if (!HasRequiredRuntimes && ShouldInstallDotnetRuntime)
            {
                await InstallDotnetRuntimes();
            }
            
            // Install PostgreSQL if needed and selected
            if (!HasPostgres && ShouldInstallPostgres)
            {
                await InstallPostgres();
            }
            
            var installLocation = new DirectoryInfo(Configuration.InstallLocation);
            
            // First, save the configuration
            await _configurationService.SaveConfigurationAsync(Configuration, installLocation);
            
            // Then start the installation/update process
            StatusMessage = IsExistingInstallation ? "Updating ImoutoRebirth..." : "Installing ImoutoRebirth...";
            
            // TODO: Implement actual installation/update code
            
            StatusMessage = IsExistingInstallation 
                ? "ImoutoRebirth has been updated successfully" 
                : "ImoutoRebirth has been installed successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = IsExistingInstallation
                ? $"Error updating ImoutoRebirth: {ex.Message}"
                : $"Error installing ImoutoRebirth: {ex.Message}";
        }
        finally
        {
            IsInstalling = false;
        }
    }
} 
