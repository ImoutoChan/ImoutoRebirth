using ImoutoViewer.ViewModel;
using System.Windows;

namespace ImoutoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //Get the arguments
            if (e.Args != null && e.Args.Length > 0)
            {
                this.Properties["ArbitraryArgName"] = e.Args[0];
            }

            //Start the main window
            MainWindowVM mainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}