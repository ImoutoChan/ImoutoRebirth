using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Installation;

public partial class InstallationStepControl : UserControl
{
    public InstallationStepControl(InstallationStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
