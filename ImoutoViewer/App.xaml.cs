using ImoutoViewer.ViewModel;
using System.Windows;

namespace ImoutoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private MainWindowVM _mainWindowVM;

        protected override void OnStartup(StartupEventArgs e)
        {
            //Get the arguments
            if (e.Args.Length > 0)
            {
                Properties["ArbitraryArgName"] = e.Args[0];
            }

            //Start the main window
            _mainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}