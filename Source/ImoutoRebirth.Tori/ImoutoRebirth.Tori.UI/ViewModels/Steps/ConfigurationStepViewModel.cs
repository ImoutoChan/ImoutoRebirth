using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImoutoRebirth.Tori.UI.ViewModels.Steps;

public partial class ConfigurationStepViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _installLocation = @"C:\Program Files\Imouto";
    
    [ObservableProperty]
    private string _danbooruLogin = string.Empty;
    
    [ObservableProperty]
    private string _danbooruApiKey = string.Empty;
    
    [ObservableProperty]
    private string _yandereLogin = string.Empty;
    
    [ObservableProperty]
    private string _yandereApiKey = string.Empty;

    [RelayCommand]
    private void BrowseInstallLocation()
    {
    }
}