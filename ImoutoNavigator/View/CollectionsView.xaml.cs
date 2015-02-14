using ImoutoNavigator.ViewModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            //parentWindow.MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;

            var result = await parentWindow.ShowInputAsync("Create collection", "Name");

            if (result == null) //user pressed cancel
                return;

            //var error = (DataContext as CollectionManagerVM).CreateCollection(result);
            //if (error != null)
            //{
            //    await parentWindow.ShowMessageAsync("Can not create collection", error);
            //}
            //else
            //{
            //    var dialog = (BaseMetroDialog)parentWindow.Resources["SuccessCreateCollectionDialog"];
            //    dialog = dialog.ShowDialogExternally();

            //    await Task.Delay(2000);

            //    await dialog.RequestCloseAsync();
            //}
        }
    }
}
