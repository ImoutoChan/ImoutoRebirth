using System;
using System.Diagnostics;
using Imouto.Viewer.ViewModel;
using System.Linq;
using System.Windows;

namespace Imouto.Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    partial class App
    {
        private const string _navGuidParam = "-nav-guid=";

        private MainWindowVM _mainWindowVM;

        protected override void OnStartup(StartupEventArgs e)
        {
            //Get the arguments
            if (e.Args.Length > 0)
            {
                var guidStr = e.Args.FirstOrDefault(x => x.StartsWith(_navGuidParam));
                if (guidStr != null)
                {
                    var guid = Guid.Parse(guidStr.Substring(_navGuidParam.Length));
                    if (guid != default(Guid))
                    {
                        Properties["NavigatorGuid"] = guid;
                    }
                }

                var filesArgs = e.Args.Where(x => !x.StartsWith(_navGuidParam));

                string result = filesArgs.Aggregate((current, arg) => current + "\n&$&\n" + arg);
                Properties["ArbitraryArgName"] = result;
            }

            //Start the main window
            _mainWindowVM = new MainWindowVM();

            base.OnStartup(e);
        }
    }
}