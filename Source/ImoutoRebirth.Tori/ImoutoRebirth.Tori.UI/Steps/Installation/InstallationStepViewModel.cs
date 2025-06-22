using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;

namespace ImoutoRebirth.Tori.UI.Steps.Installation;

public partial class InstallationStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;
    private readonly IVersionService _versionService;
    private readonly IInstaller _installer;

    [ObservableProperty]
    private string? _logString;

    public InstallationStepViewModel(
        IMessenger messenger,
        IConfigurationStorage configurationStorage,
        IVersionService versionService,
        IInstaller installer)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;
        _versionService = versionService;
        _installer = installer;

        LogConfiguration();
    }

    public string Title => "Installation";

    public int State => 5;

    [RelayCommand]
    private void Install()
    {
    }

    private void LogConfiguration()
    {
        var configuration = JsonSerializer.Serialize(
            _configurationStorage.CurrentConfiguration.WriteToDictionary(),
            new JsonSerializerOptions { WriteIndented = true });
        
        LogString = null;
        AppendLog("Preparing to install with the following configuration");
        AppendLog($"Install Location: {_configurationStorage.CurrentConfiguration.InstallLocation}");
        AppendLog($"Installing PostgreSQL: {_configurationStorage.ShouldInstallPostgreSql}");
        AppendLog($"Installing Runtimes: {_configurationStorage.ShouldInstallRuntimes}");
        AppendLog($"Current version: {_versionService.GetLocalVersion(new(_configurationStorage.CurrentConfiguration.InstallLocation))}");
        AppendLog($"New version: {_versionService.GetNewVersion()}");
        AppendLog($"Configuration:");
        AppendLog(configuration);
    }

    private void AppendLog(string str)
    {
        if (LogString == null)
            LogString = "";

        LogString += str +  Environment.NewLine;
    }

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Database));
}
