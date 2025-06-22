using System.Windows.Controls;

namespace ImoutoRebirth.Tori.UI.Steps.Installation;

public partial class InstallationStepControl : UserControl
{
    public InstallationStepControl(InstallationStepViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
        LogTextBox.CaretIndex = LogTextBox.Text.Length;
    }

    private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        LogTextBox.ScrollToEnd();
    }
}
