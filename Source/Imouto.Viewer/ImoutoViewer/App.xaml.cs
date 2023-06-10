using System.Windows;
using ImoutoViewer.ImoutoRebirth.NavigatorArgs;
using ImoutoViewer.ViewModel;

namespace ImoutoViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
partial class App
{
    private const string NavSearchParam = "-nav-search=";

    private MainWindowVM? _mainWindowVm;

    protected override void OnStartup(StartupEventArgs e)
    {
        //Get the arguments
        if (e.Args.Length > 0)
        {
            var base64String = e.Args.FirstOrDefault(x => x.StartsWith(NavSearchParam));
            if (base64String != null)
            {
                var navigatorSearchParams = Base64Serializer.Deserialize<ImoutoViewerArgs>(base64String.Substring(NavSearchParam.Length));
                ApplicationProperties.NavigatorSearchParams = navigatorSearchParams;
            }

            var fileList = e.Args.Where(x => !x.StartsWith(NavSearchParam)).ToList();
            ApplicationProperties.FileNamesToOpen = fileList;
        }

        //Start the main window
        _mainWindowVm = new MainWindowVM();

        base.OnStartup(e);
    }
}

internal static class ApplicationProperties
{
    public static ImoutoViewerArgs? NavigatorSearchParams 
    {
        get => Application.Current.Properties["NavigatorSearchParams"] as ImoutoViewerArgs;
        set => Application.Current.Properties["NavigatorSearchParams"] = value;
    }

    public static IReadOnlyCollection<string>? FileNamesToOpen 
    {
        get => Application.Current.Properties["FileNamesToOpen"] as IReadOnlyCollection<string> 
               ?? ArraySegment<string>.Empty;
        set => Application.Current.Properties["FileNamesToOpen"] = value;
    }

    public static bool BoundToNavigatorSearch
    {
        get => Application.Current.Properties["BoundToNavigatorSearch"] is bool value && value;
        set => Application.Current.Properties["BoundToNavigatorSearch"] = value;
    }
}
