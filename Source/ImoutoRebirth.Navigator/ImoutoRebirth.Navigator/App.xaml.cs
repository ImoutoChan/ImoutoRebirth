using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ImoutoRebirth.Navigator.ViewModel;

namespace ImoutoRebirth.Navigator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static Guid AppGuid = Guid.NewGuid();

    internal static MainWindowVM? MainWindowVM { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            Debug.WriteLine("Dispatcher Unhandled exception: " + e.Exception.Message);
            e.SetObserved();
        };
        
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Debug.WriteLine("Dispatcher Unhandled exception: " + e.IsTerminating + " " + e.ExceptionObject);
        };
        
        //Start the main window
        MainWindowVM = new MainWindowVM();

        base.OnStartup(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("Dispatcher Unhandled exception: " + e.Exception.Message);
        e.Handled = true;
    }
}
