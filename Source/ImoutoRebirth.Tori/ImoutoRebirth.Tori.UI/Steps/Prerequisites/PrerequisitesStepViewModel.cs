using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Tori.UI.Steps.Prerequisites;

public partial class PrerequisitesStepViewModel : ObservableObject, IStep
{
    private readonly IMessenger _messenger;
    private readonly IConfigurationStorage _configurationStorage;

    [ObservableProperty]
    private string _isDotnetAspNetRuntimeInstalled = "loading...";

    [ObservableProperty]
    private string _isDotnetDesktopRuntimeInstalled = "loading...";

    [ObservableProperty]
    private bool _isPostgresInstalled;

    [ObservableProperty]
    private bool _isPostgresPortInUse;

    [ObservableProperty] 
    private IReadOnlyCollection<InstalledPostgresInfo> _postgresWindowsServices = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ServiceName), nameof(ServiceVersion), nameof(Port))]
    private bool _isLoading;

    [ObservableProperty]
    private bool _shouldInstallPostgres;

    [ObservableProperty]
    private bool _shouldInstallRuntimes;

    [ObservableProperty]
    private bool? _isPostgresOk;

    [ObservableProperty]
    private bool? _areRuntimesOk;

    [ObservableProperty] 
    private string _shouldInstallPostgresText = "install";

    [ObservableProperty] 
    private string _shouldInstallRuntimesText = "install";

    public PrerequisitesStepViewModel(
        IDependencyManager dependencyManager,
        IOptions<PrerequisitesSettings> options,
        IMessenger messenger,
        IConfigurationStorage configurationStorage)
    {
        _messenger = messenger;
        _configurationStorage = configurationStorage;
        _ = LoadPrerequisitesStates(dependencyManager, options);
    }

    public string Title => "Prerequisites";

    public int State => 1;

    public string ServiceName => PostgresWindowsServices.FirstOrDefault()?.ServiceName ?? (IsLoading ? "loading..." : "not found");

    public string ServiceVersion => PostgresWindowsServices.FirstOrDefault()?.Version?.ToString() ?? (IsLoading ? "loading..." : "not found");

    public string Port => IsLoading ? "loading..." : IsPostgresPortInUse ? "5432" : "not used";

    private async Task LoadPrerequisitesStates(IDependencyManager dependencyManager, IOptions<PrerequisitesSettings> options)
    {
        var settings = options.Value;

        IsLoading = true;

        var loaded = await Task.Run(async () =>
        {
            var isDotnetAspNetRuntimeInstalled
                = dependencyManager.IsDotnetAspNetRuntimeInstalled(settings.DotnetRuntimeRequiredVersion);

            var isDotnetDesktopRuntimeInstalled
                = dependencyManager.IsDotnetDesktopRuntimeInstalled(settings.DotnetRuntimeRequiredVersion);

            var isPostgresInstalled = dependencyManager.IsPostgresInstalled();
            var isPostgresPortInUse = dependencyManager.IsPostgresPortInUse();

            var postgresWindowsServices = dependencyManager.GetPostgresWindowsServices();

            await Task.Delay(500);

            return new
            {
                IsDotnetAspNetRuntimeInstalled = await isDotnetAspNetRuntimeInstalled,
                IsDotnetDesktopRuntimeInstalled = await isDotnetDesktopRuntimeInstalled,
                IsPostgresInstalled = await isPostgresInstalled,
                IsPostgresPortInUse = isPostgresPortInUse,
                PostgresWindowsServices = postgresWindowsServices
            };
        });

        IsDotnetAspNetRuntimeInstalled = loaded.IsDotnetAspNetRuntimeInstalled ? "installed" : "not found";
        IsDotnetDesktopRuntimeInstalled = loaded.IsDotnetDesktopRuntimeInstalled ? "installed" : "not found";
        IsPostgresInstalled = loaded.IsPostgresInstalled;
        IsPostgresPortInUse = loaded.IsPostgresPortInUse;
        PostgresWindowsServices = loaded.PostgresWindowsServices;

        ShouldInstallPostgres = !IsPostgresInstalled && !IsPostgresPortInUse;
        ShouldInstallRuntimes = !loaded.IsDotnetAspNetRuntimeInstalled || !loaded.IsDotnetDesktopRuntimeInstalled;
        IsPostgresOk = !ShouldInstallPostgres;
        AreRuntimesOk = !ShouldInstallRuntimes;
        ShouldInstallPostgresText = ShouldInstallPostgres ? "install postgres" : "install anyway";
        ShouldInstallRuntimesText = ShouldInstallRuntimes ? "install runtimes" : "install anyway";

        IsLoading = false;
    }

    [RelayCommand]
    private void GoNext()
    {
        _configurationStorage.ShouldInstallPostgreSql = ShouldInstallPostgres;
        _configurationStorage.ShouldInstallRuntimes = ShouldInstallRuntimes;

        _messenger.Send(new NavigateTo(InstallerStep.Accounts));
    }
}

public class PrerequisitesSettings
{
    public required string DotnetRuntimeRequiredVersion { get; set; } = "9.0.6";
}
