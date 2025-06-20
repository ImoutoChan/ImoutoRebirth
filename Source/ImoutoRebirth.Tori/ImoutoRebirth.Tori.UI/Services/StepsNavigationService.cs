using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Tori.UI.Models;

namespace ImoutoRebirth.Tori.UI.Services;

public interface IStepsNavigationService
{
    InstallerStep CurrentStep { get; }
    
    ReadOnlyCollection<InstallerStep> AvailableSteps { get; }
    
    bool CanGoNext { get; }
    
    bool CanGoBack { get; }
    
    void GoToStep(InstallerStep step);
    
    void GoToNextStep();
    
    void GoToPreviousStep();
    
    event EventHandler<InstallerStep>? StepChanged;
}

public partial class StepsNavigationService : ObservableObject, IStepsNavigationService
{
    private readonly List<InstallerStep> _availableSteps;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext), nameof(CanGoBack))]
    private InstallerStep _currentStep;
    
    public StepsNavigationService()
    {
        _availableSteps = new List<InstallerStep>
        {
            InstallerStep.Welcome,
            InstallerStep.Prerequisites,
            InstallerStep.Accounts,
            InstallerStep.Locations,
            InstallerStep.Database,
            InstallerStep.Installation
        };
        
        _currentStep = InstallerStep.Welcome;
    }
    
    public ReadOnlyCollection<InstallerStep> AvailableSteps => _availableSteps.AsReadOnly();
    
    public bool CanGoNext => CurrentStep != InstallerStep.Installation;
    
    public bool CanGoBack => CurrentStep != InstallerStep.Welcome;
    
    public void GoToStep(InstallerStep step)
    {
        if (!_availableSteps.Contains(step))
            throw new ArgumentException($"Step {step} is not available");
            
        if (CurrentStep == step)
            return;
            
        CurrentStep = step;
        OnStepChanged(CurrentStep);
    }
    
    public void GoToNextStep()
    {
        if (!CanGoNext)
            return;
            
        var currentIndex = _availableSteps.IndexOf(CurrentStep);
        var nextStep = _availableSteps[currentIndex + 1];
        GoToStep(nextStep);
    }
    
    public void GoToPreviousStep()
    {
        if (!CanGoBack)
            return;
            
        var currentIndex = _availableSteps.IndexOf(CurrentStep);
        var previousStep = _availableSteps[currentIndex - 1];
        GoToStep(previousStep);
    }
    
    public event EventHandler<InstallerStep>? StepChanged;
    
    protected virtual void OnStepChanged(InstallerStep e)
    {
        StepChanged?.Invoke(this, e);
    }
}