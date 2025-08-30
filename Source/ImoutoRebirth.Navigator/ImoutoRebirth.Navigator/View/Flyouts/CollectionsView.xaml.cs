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
