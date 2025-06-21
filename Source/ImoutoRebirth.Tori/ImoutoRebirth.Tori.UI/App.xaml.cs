using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Steps.Accounts;
using ImoutoRebirth.Tori.UI.Steps.Database;
using ImoutoRebirth.Tori.UI.Steps.Locations;
using ImoutoRebirth.Tori.UI.Steps.Prerequisites;
using ImoutoRebirth.Tori.UI.Steps.Welcome;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using ImoutoRebirth.Tori.UI.Windows;
using InstallerViewModel = ImoutoRebirth.Tori.UI.Windows.InstallerViewModel;

namespace ImoutoRebirth.Tori.UI;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        var mainWindow = _serviceProvider.GetRequiredService<InstallerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<IStepViewFactory, StepViewFactory>();

        // Register windows
        services.AddTransient<InstallerWindow>();

        // Tori
        services.AddTransient<IRegistryService, RegistryService>();
        services.AddTransient<IVersionService, VersionService>();
        services.AddTransient<IDependencyManager, DependencyManager>();

        services.AddSingleton<WelcomeStepViewModel>();
        services.AddSingleton<PrerequisitesStepViewModel>();
        services.AddSingleton<AccountsStepViewModel>();
        services.AddSingleton<LocationsStepViewModel>();
        services.AddSingleton<DatabaseStepViewModel>();
        services.AddSingleton<InstallerViewModel>();

        services.AddSingleton<WelcomeStepControl>();
        services.AddSingleton<PrerequisitesStepControl>();
        services.AddSingleton<AccountsStepControl>();
        services.AddSingleton<LocationsStepControl>();
        services.AddSingleton<DatabaseStepControl>();
        services.AddSingleton<AccountsStepControl>();

        services.AddSingleton<IMessenger , WeakReferenceMessenger>();

        services.AddLogging();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
