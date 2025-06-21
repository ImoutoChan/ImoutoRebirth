using MahApps.Metro.Controls;

namespace ImoutoRebirth.Tori.UI.Windows;

public partial class InstallerWindow : MetroWindow
{
    public InstallerWindow(InstallerViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }
}