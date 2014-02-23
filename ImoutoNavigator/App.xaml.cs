using System.Linq;
using System.Windows;
using ImoutoNavigator.ViewModel;

namespace ImoutoNavigator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private MainWindowVM _mainWindowVM;

        protected override void OnStartup(StartupEventArgs e)
        {
            //Start the main window
            _mainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}
