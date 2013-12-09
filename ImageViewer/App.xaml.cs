using System.Windows;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Length > 0)
            {
                this.Properties["ArbitraryArgName"] = e.Args[0];
            }
            base.OnStartup(e);
        }
    }
}