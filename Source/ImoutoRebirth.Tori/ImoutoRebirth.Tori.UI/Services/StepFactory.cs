using System.Windows.Controls;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Steps;
using ImoutoRebirth.Tori.UI.ViewModels.Steps;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IStepFactory
{
    UserControl CreateStepControl(InstallerStep step);
}

public class StepFactory : IStepFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StepFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public UserControl CreateStepControl(InstallerStep step)
    {
        return step switch
        {
            InstallerStep.Welcome => CreateWelcomeStep(),
            InstallerStep.Accounts => CreateAccountsStep(),
            InstallerStep.Prerequisites => CreateAccountsStep(),
            InstallerStep.Locations => CreateAccountsStep(),
            InstallerStep.Database => CreateAccountsStep(),
            InstallerStep.Installation => CreateAccountsStep(),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
    }
    
    private UserControl CreateWelcomeStep()
    {
        var viewModel = _serviceProvider.GetRequiredService<WelcomeStepViewModel>();
        var control = new WelcomeStepControl { DataContext = viewModel };
        return control;
    }
    
    private UserControl CreateAccountsStep()
    {
        var viewModel = _serviceProvider.GetRequiredService<ConfigurationStepViewModel>();
        var control = new ConfigurationStepControl { DataContext = viewModel };
        return control;
    }
}