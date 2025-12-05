using System.Windows;
using System.Windows.Controls;

namespace ImoutoRebirth.Navigator.View.Flyouts;

/// <summary>
/// Interaction logic for SettingsView.xaml
/// </summary>
public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void IntegrityReportsButton_Click(object sender, RoutedEventArgs e)
    {
        var mainWindow = Window.GetWindow(this) as MainWindow;
        mainWindow?.OpenIntegrityReportsFlyout();
    }
}
