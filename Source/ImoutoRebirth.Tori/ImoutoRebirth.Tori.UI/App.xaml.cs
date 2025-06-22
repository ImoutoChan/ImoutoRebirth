using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Steps.Accounts;
using ImoutoRebirth.Tori.UI.Steps.Database;
using ImoutoRebirth.Tori.UI.Steps.Installation;
using ImoutoRebirth.Tori.UI.Steps.Locations;
using ImoutoRebirth.Tori.UI.Steps.Prerequisites;
using ImoutoRebirth.Tori.UI.Steps.Welcome;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ImoutoRebirth.Tori.Configuration;
using InstallerViewModel = ImoutoRebirth.Tori.UI.Windows.InstallerViewModel;

namespace ImoutoRebirth.Tori.UI;

public class AppSettings
{
    public bool ForcedUpdate { get; set; }

    public required DirectoryInfo UpdaterLocation { get; set; }
}

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        services.Configure<AppSettings>(x =>
        {
            x.ForcedUpdate = e.Args.Contains("force");
            x.UpdaterLocation = DetectUpdaterLocation(e.Args);
        });

        ConfigureServices(services, e.Args);
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<InstallerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services, string[] eArgs)
    {
        services.AddSingleton<IMessenger , WeakReferenceMessenger>();
        services.AddLogging();

        services.AddSingleton<IStepViewFactory, StepViewFactory>();
        services.AddTransient<IRegistryService, RegistryService>();
        services.AddTransient<IVersionService, VersionService>();
        services.AddTransient<IDependencyManager, DependencyManager>();
        services.AddTransient<IConfigurationService, ConfigurationService>();
        services.AddTransient<IInstaller, Installer>();

        services.AddSingleton<IConfigurationStorage, ConfigurationStorage>();

        services.AddSingleton<WelcomeStepViewModel>();
        services.AddSingleton<PrerequisitesStepViewModel>();
        services.AddSingleton<AccountsStepViewModel>();
        services.AddSingleton<LocationsStepViewModel>();
        services.AddSingleton<DatabaseStepViewModel>();
        services.AddSingleton<InstallerViewModel>();
        services.AddSingleton<InstallationStepViewModel>();

        services.AddTransient<InstallerWindow>();
        services.AddSingleton<WelcomeStepControl>();
        services.AddSingleton<PrerequisitesStepControl>();
        services.AddSingleton<AccountsStepControl>();
        services.AddSingleton<LocationsStepControl>();
        services.AddSingleton<DatabaseStepControl>();
        services.AddSingleton<AccountsStepControl>();
        services.AddSingleton<InstallationStepControl>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }

    private static DirectoryInfo DetectUpdaterLocation(string[] args)
    {
        if (args.Any())
        {
            var updaterLocation = new DirectoryInfo(args[0]);

            if (IsUpdaterLocation(updaterLocation))
                return updaterLocation;
        }

        var currentLocation = new DirectoryInfo(Directory.GetCurrentDirectory());

        if (IsUpdaterLocation(currentLocation))
            return currentLocation;

        currentLocation = currentLocation.Parent!;

        if (IsUpdaterLocation(currentLocation))
            return currentLocation;

        throw new InvalidOperationException(
            "Could not find updater location, please provide it as the first cli argument");

        bool IsUpdaterLocation(DirectoryInfo directoryInfo)
        {
            return directoryInfo.Exists
                   && directoryInfo.GetFiles().Any(x => x.Name == ConfigurationService.ConfigurationFilename);
        }
    }
}
