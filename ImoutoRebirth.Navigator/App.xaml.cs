using System;
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            //Start the main window
            MainWindowVM = new MainWindowVM();
            await MainWindowVM.InitializeAsync();

            base.OnStartup(e);
        }
    }
}
