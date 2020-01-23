using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator
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
