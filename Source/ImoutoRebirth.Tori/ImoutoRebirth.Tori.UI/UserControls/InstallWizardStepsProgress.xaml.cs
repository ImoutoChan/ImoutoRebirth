using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoRebirth.Tori.UI.UserControls;

public partial class InstallWizardStepsProgress : UserControl
{
    public InstallWizardStepsProgress() => InitializeComponent();

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

    public event EventHandler<int>? OnStateClicked;

    private void StateClicked1(object sender, MouseButtonEventArgs e) => OnStateClicked?.Invoke(this, 1);
    private void StateClicked2(object sender, MouseButtonEventArgs e) => OnStateClicked?.Invoke(this, 2);
    private void StateClicked3(object sender, MouseButtonEventArgs e) => OnStateClicked?.Invoke(this, 3);
    private void StateClicked4(object sender, MouseButtonEventArgs e) => OnStateClicked?.Invoke(this, 4);
    private void StateClicked5(object sender, MouseButtonEventArgs e) => OnStateClicked?.Invoke(this, 5);
}