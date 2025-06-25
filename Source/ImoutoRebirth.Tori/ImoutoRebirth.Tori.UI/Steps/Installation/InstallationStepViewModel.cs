using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Steps.Prerequisites;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Tori.UI.Steps.Installation;

public partial class InstallationStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;
    private readonly IVersionService _versionService;
    private readonly IInstaller _installer;
    private readonly ForwardingLoggerProvider _provider;
    private readonly ILogger<InstallationStepViewModel> _logger;
    private readonly IDependencyManager _dependencyManager;
    private readonly IOptions<PrerequisitesSettings> _prerequisitesSettings;
    private readonly IOptions<AppSettings> _appSettings;

    [ObservableProperty]
    private string? _logString;

    [ObservableProperty]
    private bool _isInstalling;

    [ObservableProperty]
    private bool _isInstallationStarted;

    [ObservableProperty]
    private bool _isInstallationFinished;

    [ObservableProperty]
    private int _progressValue = 0;

    public InstallationStepViewModel(
        IMessenger messenger,
        IConfigurationStorage configurationStorage,
        IVersionService versionService,
        IInstaller installer,
        ForwardingLoggerProvider provider,
        ILogger<InstallationStepViewModel> logger,
        IDependencyManager dependencyManager,
        IOptions<PrerequisitesSettings> prerequisitesSettings,
        IOptions<AppSettings> appSettings)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;
        _versionService = versionService;
        _installer = installer;
        _provider = provider;
        _logger = logger;
        _dependencyManager = dependencyManager;
        _prerequisitesSettings = prerequisitesSettings;
        _appSettings = appSettings;

        _provider.Logged += (_, s) =>
        {
            if (IsInstalling)
                AppendLog(s);
        };


        if (appSettings.Value.AutoUpdate)
        {
            _ = Install();
        }
        else
        {
            _ = LogConfiguration();
        }
    }

    public string Title => "Installation";

    public int State => 5;

    [RelayCommand]
    private async Task Install()
    {
        await LogConfiguration();

        IsInstalling = true;
        IsInstallationStarted = true;

        try
        {
            await InstallInternal();
        }
        catch (Exception e)
        {
            AppendLog(e.Message);
        }

        IsInstalling = false;
        IsInstallationFinished = true;
    }

    private async Task InstallInternal()
    {
        _logger.LogInformation("Starting installation process");

        AppendLog();
        if (_configurationStorage.ShouldInstallRuntimes)
        {
            AppendLog("Installing runtime dependencies...");
            await _dependencyManager.InstallDotnetAspNetRuntime(_prerequisitesSettings.Value.DotnetRuntimeRequiredVersion);
            await _dependencyManager.InstallDotnetDesktopRuntime(_prerequisitesSettings.Value.DotnetRuntimeRequiredVersion);
        }
        else
        {
            AppendLog("Runtime dependencies skipped...");
        }
        ProgressValue = 10;

        if (_configurationStorage.ShouldInstallPostgreSql)
        {
            AppendLog("Installing postgres dependencies...");
            await _dependencyManager.InstallPostgres();
        }
        else
        {
            AppendLog("Postgres dependencies skipped...");
        }
        ProgressValue = 20;

        await _installer.WizardInstallOrUpdate(
            _appSettings.Value.UpdaterLocation,
            _appSettings.Value.ForcedUpdate,
            _configurationStorage.CurrentConfiguration);

        _logger.LogInformation("Installation process finished");

        ProgressValue = 100;
    }

    [RelayCommand]
    public void CloseApp() => Application.Current.Shutdown();

    private async Task LogConfiguration()
    {
        await _configurationStorage.ConfigurationLoaded;

        var configuration = JsonSerializer.Serialize(
            _configurationStorage.CurrentConfiguration.WriteToDictionary(),
            new JsonSerializerOptions { WriteIndented = true });
        
        LogString = null;
        AppendLog("Preparing to install with the following configuration");
        AppendLog();
        AppendLog($"Install Location: {_configurationStorage.CurrentConfiguration.InstallLocation}");
        AppendLog($"Installing PostgreSQL: {_configurationStorage.ShouldInstallPostgreSql}");
        AppendLog($"Installing Runtimes: {_configurationStorage.ShouldInstallRuntimes}");
        AppendLog();
        AppendLog($"Current version: {await _versionService.GetLocalVersion(new(_configurationStorage.CurrentConfiguration.InstallLocation))}");
        AppendLog($"New version: {_versionService.GetNewVersion()}");
        AppendLog();
        AppendLog($"Configuration:");
        AppendLog(configuration);
        AppendLog();
    }

    private void AppendLog(string str)
    {
        if (LogString == null)
            LogString = "";

        LogString += str +  Environment.NewLine;
    }

    private void AppendLog()
    {
        if (LogString == null)
            LogString = "";

        LogString += Environment.NewLine;
        LogString += Environment.NewLine;
    }

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Database));
}
