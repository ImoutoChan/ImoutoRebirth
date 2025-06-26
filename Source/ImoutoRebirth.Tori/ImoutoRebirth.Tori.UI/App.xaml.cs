using System.IO;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ImoutoRebirth.Tori.Configuration;
using ImoutoRebirth.Tori.UI.Steps;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;

namespace ImoutoRebirth.Tori.UI;

public class AppSettings
{
    public bool ForcedUpdate { get; set; }

    public bool AutoUpdate { get; set; }

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
            x.AutoUpdate = e.Args.Contains("auto");
            x.UpdaterLocation = DetectUpdaterLocation(e.Args);
        });

        var configurationBuilder = new ConfigurationBuilder()
            .AddEnvironmentVariables();

        var configuration = configurationBuilder.Build();

        services.Configure<DependencyManagerOptions>(configuration.GetSection("DependencyManager"));

        ConfigureServices(services, e.Args);
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<InstallerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services, string[] eArgs)
    {
        var logForwarder = new ForwardingLoggerProvider();
        services.AddSingleton(logForwarder);

        services.AddSingleton<IMessenger , WeakReferenceMessenger>();
        services.AddLogging(x => x.AddProvider(logForwarder));

        services.Configure<DependencyManagerOptions>(x
            => x.ProcessConsoleOutput = y => logForwarder.CreateLogger(nameof(Startup)).LogInformation(y));

        services.AddSingleton<IStepViewFactory, StepViewFactory>();
        services.AddSingleton<IDependencyManager, DependencyManager>();

        services.AddTransient<IRegistryService, RegistryService>();
        services.AddTransient<IVersionService, VersionService>();
        services.AddTransient<IConfigurationService, ConfigurationService>();
        services.AddTransient<IInstaller, Installer>();
        services.AddTransient<IWindowsServicesManager, WindowsServicesManager>();
        services.AddTransient<IWindowsServiceUpdater, WindowsServiceUpdater>();
        services.AddTransient<IShortcutService, ShortcutService>();

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
