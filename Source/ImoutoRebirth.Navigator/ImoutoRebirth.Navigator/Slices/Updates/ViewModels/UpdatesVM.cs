using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Common;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Slices.Updates.Models;
using ImoutoRebirth.Navigator.Slices.Updates.Services;
using Serilog;

namespace ImoutoRebirth.Navigator.Slices.Updates.ViewModels;

internal partial class UpdatesVM : ObservableObject
{
    private readonly IUpdateService _updateService;

    private ReleaseInfo? _latestStable;
    private ReleaseInfo? _latestNightly;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
    [NotifyPropertyChangedFor(nameof(UpdateTo))]
    public partial UpdateChannel SelectedChannel { get; set; }

    [ObservableProperty]
    public partial string CurrentVersion { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateTo))]
    public partial string LatestStableVersion { get; private set; } = "—";

    [ObservableProperty]
    public partial string LatestStableDate { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateTo))]
    public partial string LatestNightlyVersion { get; private set; } = "—";

    [ObservableProperty]
    public partial string LatestNightlyDate { get; private set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCheckForUpdates))]
    public partial bool IsChecking { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanCheckForUpdates))]
    [NotifyPropertyChangedFor(nameof(IsUpdateAvailable))]
    public partial bool IsDownloading { get; private set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; private set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; private set; }

    [ObservableProperty]
    public partial bool HasStatusMessage { get; private set; }

    [ObservableProperty]
    public partial bool ShowUpdatesControl { get; private set; } = true;

    public bool CanCheckForUpdates => !IsChecking && !IsDownloading;

    public bool IsUpdateAvailable
    {
        get
        {
            if (IsDownloading)
                return false;

            var current = _updateService.GetCurrentVersion();
            var latestForChannel = SelectedChannel == UpdateChannel.Stable ? _latestStable : _latestNightly;

            return latestForChannel?.IsNewerThan(current) == true;
        }
    }

    public string? UpdateTo
    {
        get
        {
            var latestForChannel = SelectedChannel == UpdateChannel.Stable ? _latestStable : _latestNightly;
            return latestForChannel?.VersionDisplay.ToUpperInvariant();
        }
    }

    public IReadOnlyList<UpdateChannel> AvailableChannels { get; } = Enum.GetValues<UpdateChannel>();

    public UpdatesVM()
    {
        _updateService = ServiceLocator.GetService<IUpdateService>();

        CurrentVersion = _updateService.GetCurrentVersion().ToString().Split('+').First();
        SelectedChannel = Settings.Default.UpdateChannel.ParseEnumOrDefault<UpdateChannel>();
    }

    partial void OnSelectedChannelChanged(UpdateChannel value)
    {
        Settings.Default.UpdateChannel = value.ToString();
        OnPropertyChanged(nameof(IsUpdateAvailable));
    }

    [RelayCommand]
    private void ToggleShowUpdatesControl()
    {
        ShowUpdatesControl = !ShowUpdatesControl;
    }

    [RelayCommand(CanExecute = nameof(CanCheckForUpdates))]
    private async Task CheckForUpdatesAsync()
    {
        IsChecking = true;
        SetStatus("Checking for updates...");

        try
        {
            var latestStable = _updateService.GetLatestStableAsync();
            var latestNightly = _updateService.GetLatestAlphaAsync();
            _latestStable = await latestStable;
            _latestNightly = await latestNightly;

            UpdateVersionDisplay();
            ClearStatus();
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Network error while checking for updates");
            SetStatus("Unable to check for updates (network error)");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to check for updates");
            SetStatus("Failed to check for updates");
        }
        finally
        {
            IsChecking = false;
            OnPropertyChanged(nameof(IsUpdateAvailable));
        }
    }

    public async Task CheckForUpdatesOnStartupAsync()
    {
        try
        {
            _latestStable = await _updateService.GetLatestStableAsync();
            _latestNightly = await _updateService.GetLatestAlphaAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateVersionDisplay();
                OnPropertyChanged(nameof(IsUpdateAvailable));
            });
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to check for updates on startup");
        }
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        var latestForChannel = SelectedChannel == UpdateChannel.Stable ? _latestStable : _latestNightly;

        if (latestForChannel is null)
        {
            SetStatus("No update available");
            return;
        }

        IsDownloading = true;
        DownloadProgress = 0;
        SetStatus("Downloading update...");

        try
        {
            var fileName = Path.GetFileName(latestForChannel.InstallerUrl);
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                "ImoutoRebirth Update",
                fileName);

            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p;
                SetStatus($"Downloading... {p:F1}%");
            });

            await _updateService.DownloadInstallerAsync(
                latestForChannel.InstallerUrl,
                tempPath,
                progress);

            SetStatus("Starting installer...");

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(tempPath)
            });

            if (process is null)
            {
                Log.Warning("Failed to start installer");
                SetStatus("Failed to start installer");
                return;
            }

            await process.WaitForExitAsync();

            var installationFolder = Path.Combine(Path.GetDirectoryName(tempPath)!, fileName.Replace(".exe", ""));

            var installingProcess = Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(installationFolder, "install.cmd"),
                UseShellExecute = true,
                WorkingDirectory = installationFolder
            });

            if (installingProcess is null)
            {
                Log.Warning("Failed to start installer");
                SetStatus("Failed to start installer");
                return;
            }

            await installingProcess.WaitForExitAsync();

            // the installer will close all running instances of the ImoutoNavigator
        }
        catch (HttpRequestException ex)
        {
            Log.Error(ex, "Network error while downloading update");
            SetStatus("Failed to download update. Please check your network connection and try again.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to download or start update");
            SetStatus("Failed to download update. Please try again.");
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
        }
    }

    private void UpdateVersionDisplay()
    {
        if (_latestStable is not null)
        {
            LatestStableVersion = _latestStable.VersionDisplay;
            LatestStableDate = _latestStable.PublishedAt.ToLocalTime().ToString("dd MMM yyyy");
        }
        else
        {
            LatestStableVersion = "—";
            LatestStableDate = string.Empty;
        }

        if (_latestNightly is not null)
        {
            LatestNightlyVersion = _latestNightly.VersionDisplay;
            LatestNightlyDate = _latestNightly.PublishedAt.ToLocalTime().ToString("dd MMM yyyy");
        }
        else
        {
            LatestNightlyVersion = "—";
            LatestNightlyDate = string.Empty;
        }
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        HasStatusMessage = true;
    }

    private void ClearStatus()
    {
        StatusMessage = null;
        HasStatusMessage = false;
    }
}
