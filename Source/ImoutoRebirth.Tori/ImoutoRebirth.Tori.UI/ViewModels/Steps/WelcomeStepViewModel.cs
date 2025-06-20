using System.Security.Policy;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Tori.Services;

namespace ImoutoRebirth.Tori.UI.ViewModels.Steps;

public partial class WelcomeStepViewModel : ObservableObject, IStep
{
    [ObservableProperty]
    private string _title = "ImoutoRebirth";
    
    [ObservableProperty]
    private string _currentVersion;

    [ObservableProperty]
    private string _newVersion;

    [ObservableProperty]
    private bool _isUpdating;

    public WelcomeStepViewModel(IRegistryService registryService, IVersionService versionService)
    {
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
}