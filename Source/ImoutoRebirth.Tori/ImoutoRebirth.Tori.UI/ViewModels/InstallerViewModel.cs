using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Services;

namespace ImoutoRebirth.Tori.UI.ViewModels;

public partial class InstallerViewModel : ViewModelBase
{
    private readonly IStepsNavigationService _navigationService;
    
    [ObservableProperty]
    private string _windowTitle = "ImoutoRebirth Installer";

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    public InstallerViewModel(IStepsNavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.StepChanged += (_, step) => UpdateWindowTitle(step);
    }

    public InstallerStep CurrentStep => _navigationService.CurrentStep;
    
    /// <summary>
    /// For binding in XAML
    /// </summary>
    public IStepsNavigationService NavigationService => _navigationService;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => _navigationService.GoToNextStep();

    private bool CanGoNext() => _navigationService.CanGoNext;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back() => _navigationService.GoToPreviousStep();

    private bool CanGoBack() => _navigationService.CanGoBack;

    [RelayCommand(CanExecute = nameof(CanGoToStep))]
    private void GoToStep(InstallerStep step) => _navigationService.GoToStep(step);

    private bool CanGoToStep(InstallerStep step) =>
        _navigationService.AvailableSteps.Contains(step) && 
        _navigationService.AvailableSteps.IndexOf(step) <= _navigationService.AvailableSteps.IndexOf(_navigationService.CurrentStep);

    private void UpdateWindowTitle(InstallerStep step)
    {
        WindowTitle = $"ImoutoRebirth Installer - {step}";
        OnPropertyChanged(nameof(CurrentStep));
    }
}