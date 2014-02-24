using System.Linq;
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
                string result = e.Args.Aggregate("", (current, arg) => current + ("\n&$&\n" + arg));
                Properties["ArbitraryArgName"] = result;
            }

            //Start the main window
            _mainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}