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
        if (Window.GetWindow(this) is not MainWindow parentWindow
            || DataContext is not CollectionManagerVm { SelectedCollection: { } collection })
            return;

        var name = await parentWindow.ShowInputAsync("Rename collection", "New name");
        if (name == null)
            return;

        try
        {
            await collection.Rename(name);
        }
        catch (Exception ex)
        {
            await parentWindow.ShowMessageAsync("Cannot rename collection", ex.Message);
            return;
        }

        var dialog = (BaseMetroDialog)parentWindow.Resources["SuccessRenameCollectionDialog"];
        dialog = dialog.ShowDialogExternally();
        await Task.Delay(1500);
        await dialog.RequestCloseAsync();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CollectionManagerVm vm || vm.SelectedCollection == null)
            return;

        var parentWindow = Window.GetWindow(this) as MainWindow;
        if (parentWindow == null)
            return;

        var settings = new MetroDialogSettings
        {
            AffirmativeButtonText = "Yes",
            NegativeButtonText = "No",
            ColorScheme = MetroDialogColorScheme.Accented,
            AnimateShow = false,
            AnimateHide = false
        };

        var result = await parentWindow.ShowMessageAsync(
            "Delete collection",
            $"Are you sure you want to delete collection '{vm.SelectedCollection.Name}'?",
            MessageDialogStyle.AffirmativeAndNegative,
            settings);

        if (result != MessageDialogResult.Affirmative)
            return;

        // Execute the original RemoveCommand
        if (vm.RemoveCommand.CanExecute(null))
            await vm.RemoveCommand.ExecuteAsync(null);
    }
}
