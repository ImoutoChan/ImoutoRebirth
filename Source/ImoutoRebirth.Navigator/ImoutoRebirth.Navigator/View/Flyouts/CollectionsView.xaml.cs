using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.Slices.CreateCollectionWizard;
using ImoutoRebirth.Navigator.ViewModel.SettingsSlice;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoRebirth.Navigator.View.Flyouts;

/// <summary>
/// Interaction logic for CollectionsView.xaml
/// </summary>
public partial class CollectionsView
{
    public CollectionsView()
    {
        InitializeComponent();
    }

    private void CreateButton_Click(object sender, RoutedEventArgs e)
    {
        ServiceLocator.GetService<IMessenger>().Send<OpenCreateCollectionWizardRequest>();
    }

    private async void RenameButton_Click(object sender, RoutedEventArgs e)
    {
        var parentWindow = Window.GetWindow(this) as MainWindow;
        //parentWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

        var result = await parentWindow.ShowInputAsync("Rename collection", "New name");

        if (result == null || DataContext is not CollectionManagerVm vm) //user pressed cancel
            return;

        var error = vm.Rename(result);
        if (error != null)
        {
            await parentWindow.ShowMessageAsync("Can not create collection", error);
        }
        else
        {
            var dialog = (BaseMetroDialog)(parentWindow!.Resources["SuccessCreateCollectionDialog"])!;
            dialog = dialog.ShowDialogExternally();

            await Task.Delay(1500);

            await dialog.RequestCloseAsync();
        }
    }
}
