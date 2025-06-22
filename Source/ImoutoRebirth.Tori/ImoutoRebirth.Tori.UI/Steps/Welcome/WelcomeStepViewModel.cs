using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.Services;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;

namespace ImoutoRebirth.Tori.UI.Steps.Welcome;

public partial class WelcomeStepViewModel : ObservableObject, IStep
{
    private readonly IMessenger _messenger;
    
    [ObservableProperty]
    private string _currentVersion;

    [ObservableProperty]
    private string _newVersion;

    [ObservableProperty]
    private bool _isUpdating;

    public WelcomeStepViewModel(IRegistryService registryService, IVersionService versionService, IMessenger messenger)
    {
        _messenger = messenger;
        if (registryService.IsInstalled(out var installLocation))
        {
            CurrentVersion = versionService.GetLocalVersion(installLocation);
            IsUpdating = true;
        }
        else
        {
            CurrentVersion = "not found";
            IsUpdating = false;
        }

        NewVersion = versionService.GetNewVersion();
    }

    public string Title => "ImoutoRebirth";

    public int State => 0;

    [RelayCommand]
    public void Customize()
    {
        _messenger.Send(new NavigateTo(InstallerStep.Prerequisites));
    }

    [RelayCommand]
    public void Update()
    {
        _messenger.Send(new NavigateTo(InstallerStep.Installation));
    }
}