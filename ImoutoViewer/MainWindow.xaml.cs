using ImoutoViewer.ViewModel;
using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImoutoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Fields

        private bool _isFullscreen;
        private WindowState _lastState;

        #endregion // Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            
            RenderOptions.SetBitmapScalingMode(ViewPort, BitmapScalingMode.Fant);
        }

        #endregion //Constructors

        #region Event handlers

        //Disable speciale handling for ALT (menu button, change focus)
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewKeyDown(e);
            }
        }

        private void ScrollViewerObject_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //Enable or disable scrolling by mouse wheel (without any modify key)
            var dataContext = DataContext as MainWindowVM;
            if (dataContext != null)
            {
                dataContext.IsSimpleWheelNavigationEnable = (ScrollViewerObject.ComputedVerticalScrollBarVisibility != Visibility.Visible);
            }
        }

        //Open setting flyout
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Flyout item in Flyouts.Items)
            {
                if (item.Name == "SettingFlyout")
                {
                    item.IsOpen = true;
                }
            }
        }

        //Close all flyouts
        private void Client_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (Flyout item in Flyouts.Items)
            {
                if (item.IsOpen)
                {
                    e.Handled = true;
                }
                item.IsOpen = false;
            }
            Client.Focus();
        }

        //Disable click commands, when mouse over flyout
        private void SettingFlyout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        //Full screen
        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F11
                && !(e.Key == Key.Enter && e.Key == Key.LeftAlt || e.Key == Key.RightAlt))
            {
                return;
            }

            if (_isFullscreen)
            {
                WindowState = WindowState.Normal;
                UseNoneWindowStyle = false;
                Topmost = false;
                ShowTitleBar = true;
                WindowState = _lastState;

                ShowMinButton = true;
                ShowMaxRestoreButton = true;
                ShowCloseButton = true;

                _isFullscreen = false;
            }
            else
            {
                _lastState = WindowState;

                WindowState = WindowState.Normal;
                UseNoneWindowStyle = true;
                Topmost = true;
                ShowTitleBar = false;

                ShowMinButton = false;
                ShowMaxRestoreButton = false;
                ShowCloseButton = false;

                WindowState = WindowState.Maximized;

                _isFullscreen = true;
            }
        }

        #endregion //Event handlers
        
    }
}