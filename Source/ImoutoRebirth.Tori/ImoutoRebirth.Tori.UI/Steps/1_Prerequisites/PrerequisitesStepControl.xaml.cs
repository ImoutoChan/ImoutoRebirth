using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps;

public partial class PrerequisitesStepControl : UserControl
{
    public PrerequisitesStepControl(PrerequisitesStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
