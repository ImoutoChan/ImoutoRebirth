using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps;

public partial class DatabaseStepControl : UserControl
{
    public DatabaseStepControl(DatabaseStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
