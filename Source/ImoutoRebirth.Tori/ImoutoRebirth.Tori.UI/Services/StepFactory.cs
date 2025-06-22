using ImoutoRebirth.Tori.UI.Steps.Accounts;
using ImoutoRebirth.Tori.UI.Steps.Database;
using ImoutoRebirth.Tori.UI.Steps.Installation;
using ImoutoRebirth.Tori.UI.Steps.Locations;
using ImoutoRebirth.Tori.UI.Steps.Prerequisites;
using ImoutoRebirth.Tori.UI.Steps.Welcome;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IStep
{
    string Title { get; }

    int State { get; }
}

public enum InstallerStep
{
    Welcome = 0,
    Prerequisites = 1,
    Accounts = 2,
    Locations = 3,
    Database = 4,
    Installation = 5
}

public interface IStepViewFactory
{
    UserControl CreateStepControl(InstallerStep step);
}

public class StepViewFactory : IStepViewFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StepViewFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public UserControl CreateStepControl(InstallerStep step)
    {
        return step switch
        {
            InstallerStep.Welcome => _serviceProvider.GetRequiredService<WelcomeStepControl>(),
            InstallerStep.Prerequisites => _serviceProvider.GetRequiredService<PrerequisitesStepControl>(),
            InstallerStep.Accounts => _serviceProvider.GetRequiredService<AccountsStepControl>(),
            InstallerStep.Locations => _serviceProvider.GetRequiredService<LocationsStepControl>(),
            InstallerStep.Database => _serviceProvider.GetRequiredService<DatabaseStepControl>(),
            InstallerStep.Installation => _serviceProvider.GetRequiredService<InstallationStepControl>(),
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, null)
        };
    }
}