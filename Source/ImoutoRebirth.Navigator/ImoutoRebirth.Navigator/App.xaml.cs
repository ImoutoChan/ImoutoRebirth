using System.IO;
using System.Windows;
using System.Windows.Threading;
using ImoutoRebirth.Navigator.ViewModel;
using Serilog;

namespace ImoutoRebirth.Navigator;

public partial class App : Application
{
    public App() => Startup += async (_, _) =>
    {
        try
        {
            await RunApp();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error during application startup");
            Application.Current.Shutdown();
        }
    };

    private async Task RunApp()
    {
        var splashScreen = new SplashScreen(@"\Resources\Icon\splashscreen.png");
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
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ImoutoRebirth.Navigator", "logs");

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(logDirectory, "log.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 10)
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .CreateLogger();

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Dispatcher Unhandled exception");
            e.SetObserved();
        };
        
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Log.Error((Exception)e.ExceptionObject, "Dispatcher Unhandled exception: " + e.IsTerminating);
        };

        base.OnStartup(startupEventArgs);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Dispatcher Unhandled exception");
        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application Exiting");
        Log.CloseAndFlush();

        base.OnExit(e);
    }
}
