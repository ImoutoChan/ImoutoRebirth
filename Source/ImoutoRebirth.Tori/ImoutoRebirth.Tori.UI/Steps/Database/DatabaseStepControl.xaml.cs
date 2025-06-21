using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Database;

public partial class DatabaseStepControl : UserControl
{
    public DatabaseStepControl(DatabaseStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
