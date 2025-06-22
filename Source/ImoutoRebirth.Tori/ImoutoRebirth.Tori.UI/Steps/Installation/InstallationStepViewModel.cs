using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;

namespace ImoutoRebirth.Tori.UI.Steps.Installation;

public partial class InstallationStepViewModel : ObservableValidator, IStep
{
    private readonly IMessenger _messenger;

    public InstallationStepViewModel(IMessenger messenger, ConfigurationToInstallStorage configurationToInstallStorage)
    {
        _messenger = messenger;

        var currentConfiguration = configurationToInstallStorage.CurrentConfiguration;

        if (currentConfiguration != null)
        {
        }
    }

    public string Title => "Installation";

    public int State => 5;

    [RelayCommand]
    private void Install()
    {
    }

    [RelayCommand]
    private void GoBack()
        => _messenger.Send(new NavigateTo(InstallerStep.Database));
}
