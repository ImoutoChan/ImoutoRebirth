using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps;

public partial class LocationsStepControl : UserControl
{
    public LocationsStepControl(LocationsStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
