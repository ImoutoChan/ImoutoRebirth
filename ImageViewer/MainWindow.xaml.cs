using ImageViewer.Model;
using ImageViewer.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfAnimatedGif;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        #region Fields

        private ResizeType _currentResizeType = ResizeType.Default;

        #endregion //Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            
            RenderOptions.SetBitmapScalingMode(ViewPort, BitmapScalingMode.Fant);
        }

        #endregion //Constructors

        #region Event handlers
        
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ScrollViewerObject.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Visible
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollViewerObject.ScrollToHorizontalOffset(offset + ScrollViewerObject.HorizontalOffset);
            }
            else if (ScrollViewerObject.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible
                        && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollViewerObject.ScrollToVerticalOffset(offset + ScrollViewerObject.VerticalOffset);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            //Disable speciale handling for ALT (menu button, change focus)
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
            (this.DataContext as MainWindowVM).IsSimpleWheelNavigationEnable = (this.ScrollViewerObject.ComputedVerticalScrollBarVisibility != System.Windows.Visibility.Visible);
        }

        #endregion //Event handlers

        #region Methods
        
        private static bool IsImage(string file)
        {
            CultureInfo ci = new CultureInfo("en-US");
            string formats = @".jpg|.png|.jpeg|.bmp|.gif|.tiff";
            bool result = false;
            foreach (var item in formats.Split('|'))
            {
                result = result || file.EndsWith(item, true, ci);
                if (result) break;
            }

            return result;
        }

        #endregion //Methods

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Flyout item in this.Flyouts.Items)
            {
                if (item.Name == "SettingFlyout")
                {
                    item.IsOpen = true;
                }
            }
        }
    }

    public class ExtScrollViewer : ScrollViewer
    {
        public ExtScrollViewer() : base()
        {
            this.Loaded += ExtScrollViewer_Loaded;            
        }

        void ExtScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            (this.Content as FrameworkElement).SizeChanged += ExtScrollViewer_SizeChanged;
        }

        private void ExtScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            
            //Detect zoom
            e.

//            throw new NotImplementedException();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (this.ComputedHorizontalScrollBarVisibility == Visibility.Visible
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;

                this.ScrollToHorizontalOffset(offset + this.HorizontalOffset);
            }
            else
            {
                base.OnMouseWheel(e);
            }

            e.Handled = false;
        }
    }
}