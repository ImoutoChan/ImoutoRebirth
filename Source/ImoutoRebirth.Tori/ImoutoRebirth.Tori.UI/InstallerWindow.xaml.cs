using System.Windows;
using ImoutoRebirth.Tori.UI.Models;
using ImoutoRebirth.Tori.UI.Services;
using ImoutoRebirth.Tori.UI.ViewModels;
using MahApps.Metro.Controls;

namespace ImoutoRebirth.Tori.UI;

public partial class InstallerWindow : MetroWindow
{
    private readonly IStepFactory _stepFactory;
    private readonly IStepsNavigationService _navigationService;
    private readonly InstallerViewModel _viewModel;
    
    public InstallerWindow(IStepFactory stepFactory, IStepsNavigationService navigationService)
    {
        InitializeComponent();

        _stepFactory = stepFactory;
        _navigationService = navigationService;
        
        // Create the view model with dependency injection
        _viewModel = new InstallerViewModel(_navigationService);
        DataContext = _viewModel;
        
        // Listen for step changes
        _navigationService.StepChanged += NavigationServiceOnStepChanged;
        
        // Initialize with the first step
        NavigationServiceOnStepChanged(this, _navigationService.CurrentStep);
    }

    private void NavigationServiceOnStepChanged(object? sender, InstallerStep step)
    {
        // Create the appropriate step control
        var stepControl = _stepFactory.CreateStepControl(step);
        
        // Set it as the content
        StepContent.Content = stepControl;
    }
}