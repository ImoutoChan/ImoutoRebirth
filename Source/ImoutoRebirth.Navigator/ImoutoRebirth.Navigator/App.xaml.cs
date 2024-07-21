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
    public App() => Startup += async (_, _) => await RunApp();

    private async Task RunApp()
    {
        var splashScreen = new SplashScreen(@"\Resources\Icon\appicon.png");
        splashScreen.Show(autoClose: false, topMost: false);
        
        MainWindowVM = new MainWindowVM();
        await MainWindowVM.InitializeContextAsync();
        MainWindowVM.ShowWindow();
        
        splashScreen.Close(fadeoutDuration: TimeSpan.FromMilliseconds(100));
    }

    public static Guid AppGuid = Guid.NewGuid();

    internal static MainWindowVM? MainWindowVM { get; private set; }

    protected override void OnStartup(StartupEventArgs startupEventArgs)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Debug.WriteLine("Dispatcher Unhandled exception: " + e.Exception.Message);
            e.SetObserved();
        };
        
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Debug.WriteLine("Dispatcher Unhandled exception: " + e.IsTerminating + " " + e.ExceptionObject);
        };

        base.OnStartup(startupEventArgs);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Debug.WriteLine("Dispatcher Unhandled exception: " + e.Exception.Message);
        e.Handled = true;
    }
}
