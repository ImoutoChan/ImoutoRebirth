using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.ViewModels.Steps;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ImoutoRebirth.Tori.UI;

public partial class App : Application
{
    private ServiceProvider _serviceProvider = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        // Create and show the main window
        var mainWindow = _serviceProvider.GetRequiredService<InstallerWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register services
        services.AddSingleton<IStepsNavigationService, StepsNavigationService>();
        services.AddSingleton<IStepFactory, StepFactory>();
        
        // Register windows
        services.AddTransient<InstallerWindow>();
        
        // Tori
        services.AddTransient<IRegistryService, RegistryService>();
        services.AddTransient<IVersionService, VersionService>();

        services.AddTransient<WelcomeStepViewModel>();
        services.AddTransient<ConfigurationStepViewModel>();
    }
    
    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider.Dispose();
        base.OnExit(e);
    }
}
