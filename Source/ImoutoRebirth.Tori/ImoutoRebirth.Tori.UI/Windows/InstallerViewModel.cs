using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Steps.Installation;

namespace ImoutoRebirth.Tori.UI.Windows;

public record NavigateTo(InstallerStep Step);

public partial class InstallerViewModel : ObservableObject
{
    private readonly IStepViewFactory _viewFactory;

    [ObservableProperty]
    private InstallerStep? _currentStep;

    [ObservableProperty]
    private UserControl? _currentStepControl;
    
    public InstallerViewModel(IMessenger messenger, IStepViewFactory viewFactory)
    {
        messenger.Register<NavigateTo, InstallerViewModel>(this, (r, m) => r.GoToStep(m.Step));
        _viewFactory = viewFactory;

        GoToStep(InstallerStep.Welcome);
    }

    private void GoToStep(InstallerStep step)
    {
        if (CurrentStep == step)
            return;

        var isInstallationStarted = CurrentStep == InstallerStep.Installation
                                    && ((InstallationStepViewModel)CurrentStepControl!.DataContext)
                                    .IsInstallationStarted;
        if (isInstallationStarted)
            return;
            
        CurrentStep = step;
        CurrentStepControl = _viewFactory.CreateStepControl(step);
    }
}
