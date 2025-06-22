using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Steps.Installation;
using Microsoft.Extensions.Options;

namespace ImoutoRebirth.Tori.UI.Windows;

public record NavigateTo(InstallerStep Step);

public partial class InstallerViewModel : ObservableObject
{
    private readonly IStepViewFactory _viewFactory;
    private readonly IOptions<AppSettings> _appSettings;

    [ObservableProperty]
    private InstallerStep? _currentStep;

    [ObservableProperty]
    private UserControl? _currentStepControl;
    
    public InstallerViewModel(IMessenger messenger, IStepViewFactory viewFactory, IOptions<AppSettings> appSettings)
    {
        messenger.Register<NavigateTo, InstallerViewModel>(this, (r, m) => r.GoToStep(m.Step));
        _viewFactory = viewFactory;
        _appSettings = appSettings;

        if (_appSettings.Value.AutoUpdate)
        {
            GoToStep(InstallerStep.Installation);
        }
        else
        {
            GoToStep(InstallerStep.Welcome);
        }
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
