using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Welcome;

public partial class WelcomeStepControl : UserControl
{
    public WelcomeStepControl(WelcomeStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
