using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using Microsoft.Extensions.Options;
using InstallationStepViewModel = ImoutoRebirth.Tori.UI.Steps.InstallationStepViewModel;

namespace ImoutoRebirth.Tori.UI.Windows;

public record NavigateTo(InstallerStep Step);

public partial class InstallerViewModel : ObservableObject
{
    private readonly IStepViewFactory _viewFactory;
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IConfigurationStorage _configurationStorage;

    [ObservableProperty]
    private InstallerStep? _currentStep;

    [ObservableProperty]
    private UserControl? _currentStepControl;

    public InstallerViewModel(
        IMessenger messenger,
        IStepViewFactory viewFactory,
        IOptions<AppSettings> appSettings,
        IConfigurationStorage configurationStorage)
    {
        messenger.Register<NavigateTo, InstallerViewModel>(this, (r, m) => r.GoToStep(m.Step));
        _viewFactory = viewFactory;
        _appSettings = appSettings;
        _configurationStorage = configurationStorage;

        _ = Initialize();
    }

    private async Task Initialize()
    {
        await _configurationStorage.ConfigurationLoaded;
        if (_configurationStorage is { IsUpdating: true, CurrentConfiguration.WasMigrated: false })
        {
            if (_appSettings.Value.AutoUpdate)
            {
                GoToStep(InstallerStep.Installation);
            }
            else
            {
                GoToStep(InstallerStep.Welcome);
            }
        }
        else
        {
            GoToStep(InstallerStep.Prerequisites);
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
