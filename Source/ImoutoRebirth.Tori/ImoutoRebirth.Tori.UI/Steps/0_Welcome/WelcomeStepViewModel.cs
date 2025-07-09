using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.Windows;

namespace ImoutoRebirth.Tori.UI.Steps;

public partial class WelcomeStepViewModel : ObservableObject, IStep
{
    private readonly IConfigurationStorage _configurationStorage;
    private readonly IMessenger _messenger;
    
    [ObservableProperty]
    private string _currentVersion = "loading...";

    [ObservableProperty]
    private string _newVersion = "loading...";

    [ObservableProperty]
    private bool _isUpdating;

    public WelcomeStepViewModel(IConfigurationStorage configurationStorage, IMessenger messenger)
    {
        _configurationStorage = configurationStorage;
        _messenger = messenger;

        _ = FillProperties();
    }

    public string Title => "ImoutoRebirth";

    public int State => 0;

    [RelayCommand]
    private void Customize() => _messenger.Send(new NavigateTo(InstallerStep.Prerequisites));

    [RelayCommand]
    private void Update() => _messenger.Send(new NavigateTo(InstallerStep.Installation));

    private async Task FillProperties()
    {
        await _configurationStorage.ConfigurationLoaded;

        CurrentVersion = _configurationStorage.CurrentVersion;
        IsUpdating = _configurationStorage.IsUpdating;
        NewVersion = _configurationStorage.NewVersion;
    }
}
