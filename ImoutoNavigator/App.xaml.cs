using System;
using System.Windows;
using Imouto.Navigator.ViewModel;

namespace Imouto.Navigator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Guid AppGuid = Guid.NewGuid();

        internal static MainWindowVM MainWindowVM { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //Start the main window
            MainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}
