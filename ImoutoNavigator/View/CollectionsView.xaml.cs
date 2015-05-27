using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ImoutoNavigator.ViewModel;
using MahApps.Metro.Controls.Dialogs;

namespace ImoutoNavigator.View
{
    /// <summary>
    /// Interaction logic for CollectionsView.xaml
    /// </summary>
    public partial class CollectionsView : UserControl
    {
        public CollectionsView()
        {
            InitializeComponent();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            //parentWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

            var result = await parentWindow.ShowInputAsync("Create collection", "Name");

            if (result == null) //user pressed cancel
                return;

            var error = (DataContext as CollectionManagerVM).CreateCollection(result);
            if (error != null)
            {
                await parentWindow.ShowMessageAsync("Can not create collection", error);
            }
            else
            {
                var dialog = (BaseMetroDialog)parentWindow.Resources["SuccessCreateCollectionDialog"];
                dialog = dialog.ShowDialogExternally();

                await Task.Delay(500);

                await dialog.RequestCloseAsync();
            }
        }

        private async void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            //parentWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

            var result = await parentWindow.ShowInputAsync("Rename collection", "New name");

            if (result == null) //user pressed cancel
                return;

            var error = (DataContext as CollectionManagerVM).Rename(result);
            if (error != null)
            {
                await parentWindow.ShowMessageAsync("Can not create collection", error);
            }
            else
            {
                var dialog = (BaseMetroDialog)parentWindow.Resources["SuccessCreateCollectionDialog"];
                dialog = dialog.ShowDialogExternally();

                await Task.Delay(1500);

                await dialog.RequestCloseAsync();
            }
        }
    }
}
