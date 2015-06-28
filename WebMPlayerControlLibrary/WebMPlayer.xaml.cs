using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace Imouto.WebMPlayerControlLibrary
{
    /// <summary>
    /// Interaction logic for WebMPlayer.xaml
    /// </summary>
    public partial class WebMPlayer : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
                    DependencyProperty.Register("Source", typeof(string), typeof(WebMPlayer), new UIPropertyMetadata(null, OnWebMPlayerChanged));

        public int Source
        {
            get { return (int)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnWebMPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newPropertyValue = (string)e.NewValue;
            var webMPlayer = (WebMPlayer)d;

            if (newPropertyValue == null)
            {
                webMPlayer.ChromeBrowser.Dispose();
            }
            else
            {
                webMPlayer.ChromeBrowser.Address = newPropertyValue;
            }
        }

        public WebMPlayer()
        {
            InitializeComponent();
            ChromeBrowser.NavStateChanged += ChromeBrowser_NavStateChanged;
        }

        private void ChromeBrowser_NavStateChanged(object sender, NavStateChangedEventArgs e)
        {
            if (e.IsLoading != false)
            {
                return;
            }

            var browser = sender as ChromiumWebBrowser;
            browser.EvaluateScriptAsync("document.getElementsByTagName(\"video\")[0].setAttribute(\"loop\", \"\");");
            browser.EvaluateScriptAsync("document.getElementsByTagName(\"video\")[0].setAttribute(\"width\", \"100%\");");
        }
    }
}
