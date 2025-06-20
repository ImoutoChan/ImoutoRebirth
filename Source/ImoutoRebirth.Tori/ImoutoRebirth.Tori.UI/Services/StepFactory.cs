using System.Windows.Controls;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Steps;
using ImoutoRebirth.Tori.UI.Steps.Prerequisites;
using ImoutoRebirth.Tori.UI.Steps.Welcome;
using ImoutoRebirth.Tori.UI.ViewModels.Steps;
using Microsoft.Extensions.DependencyInjection;
using PrerequisitesStepViewModel = ImoutoRebirth.Tori.UI.Steps.Prerequisites.PrerequisitesStepViewModel;
using WelcomeStepViewModel = ImoutoRebirth.Tori.UI.Steps.Welcome.WelcomeStepViewModel;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IStepFactory
{
    UserControl CreateStepControl(InstallerStep step);
}

public class StepFactory : IStepFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StepFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public UserControl CreateStepControl(InstallerStep step)
    {
        return step switch
        {
            InstallerStep.Welcome => CreateWelcomeStep(),
            InstallerStep.Prerequisites => CreatePrerequisitesStep(),
            InstallerStep.Accounts => CreateAccountsStep(),
            InstallerStep.Locations => CreateAccountsStep(),
            InstallerStep.Database => CreateAccountsStep(),
            InstallerStep.Installation => CreateAccountsStep(),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
    }
    
    private UserControl CreateWelcomeStep()
    {
        var viewModel = _serviceProvider.GetRequiredService<WelcomeStepViewModel>();
        return new WelcomeStepControl { DataContext = viewModel };
    }
    
    private UserControl CreatePrerequisitesStep()
    {
        var viewModel = _serviceProvider.GetRequiredService<PrerequisitesStepViewModel>();
        return new PrerequisitesStepControl { DataContext = viewModel };
    }
    
    private UserControl CreateAccountsStep()
    {
        var viewModel = _serviceProvider.GetRequiredService<ConfigurationStepViewModel>();
        return new ConfigurationStepControl { DataContext = viewModel };
    }
}