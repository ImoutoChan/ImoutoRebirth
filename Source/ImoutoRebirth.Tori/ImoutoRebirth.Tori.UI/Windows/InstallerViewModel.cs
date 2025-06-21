using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;

namespace ImoutoRebirth.Tori.UI.Windows;

public record NavigateTo(InstallerStep Step);

public partial class InstallerViewModel : ObservableObject
{
    private readonly IStepViewFactory _viewFactory;

    [ObservableProperty]
    private InstallerStep _currentStep;

    [ObservableProperty]
    private UserControl? _currentStepControl;
    
    public InstallerViewModel(IMessenger messenger, IStepViewFactory viewFactory)
    {
        messenger.Register<NavigateTo, InstallerViewModel>(this, (r, m) => r.GoToStep(m.Step));
        _viewFactory = viewFactory;

        GoToStep(InstallerStep.Welcome);
    }
    
    public void GoToStep(InstallerStep step)
    {
        if (CurrentStep == step)
            return;
            
        CurrentStep = step;
        CurrentStepControl = _viewFactory.CreateStepControl(step);
    }
}