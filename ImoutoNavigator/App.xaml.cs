using System.Windows;
using ImoutoNavigator.ViewModel;

namespace ImoutoNavigator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static MainWindowVM MainWindowVM { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Start the main window
            MainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}
