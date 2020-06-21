using System.Linq;
using System.Windows;
using ImoutoViewer.ImoutoRebirthNavigator.NavigatorArgs;
using ImoutoViewer.ViewModel;

namespace ImoutoViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    partial class App
    {
        private const string NavSearchParam = "-nav-search=";
        private MainWindowVM _mainWindowVm;

        protected override void OnStartup(StartupEventArgs e)
        {
            //Get the arguments
            if (e.Args.Length > 0)
            {
                var base64String = e.Args.FirstOrDefault(x => x.StartsWith(NavSearchParam));
                if (base64String != null)
                {
                    var navigatorSearchParams = Base64Serializer.Deserialize<ImoutoViewerArgs>(base64String);

                    Properties["NavigatorSearchParams"] = navigatorSearchParams;
                }

                var filesArgs = e.Args.Where(x => !x.StartsWith(NavSearchParam));

                string result = filesArgs.Aggregate((current, arg) => current + "\n&$&\n" + arg);
                Properties["ArbitraryArgName"] = result;
            }

            //Start the main window
            _mainWindowVm = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}