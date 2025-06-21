using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Locations;

public partial class LocationsStepControl : UserControl
{
    public LocationsStepControl(LocationsStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
