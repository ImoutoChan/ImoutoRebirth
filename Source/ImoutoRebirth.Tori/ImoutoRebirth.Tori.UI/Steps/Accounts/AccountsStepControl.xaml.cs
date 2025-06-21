using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Accounts;

public partial class AccountsStepControl : UserControl
{
    public AccountsStepControl(AccountsStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}
