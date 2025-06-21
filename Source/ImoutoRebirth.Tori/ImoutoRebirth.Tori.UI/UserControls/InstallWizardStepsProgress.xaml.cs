using System.Windows;
using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.UserControls;

public partial class InstallWizardStepsProgress : UserControl
{
    public InstallWizardStepsProgress() => InitializeComponent();

    // dependency property for progress int State
    public static readonly DependencyProperty StateProperty 
        = DependencyProperty.Register(
            nameof(State), 
            typeof(int), 
            typeof(InstallWizardStepsProgress), 
            new UIPropertyMetadata(1));

    public int State
    {
        get => (int) GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }
}