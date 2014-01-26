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
            (this.DataContext as MainWindowVM).IsSimpleWheelNavigationEnable = (this.ScrollViewerObject.ComputedVerticalScrollBarVisibility != System.Windows.Visibility.Visible);
        }

        //Open setting flyout
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

        //Close all flyouts
        private void Client_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            foreach (Flyout item in this.Flyouts.Items)
            {
                if (item.IsOpen)
                {
                    e.Handled = true;
                }
                item.IsOpen = false;
            }
        }

        //Disable click commands, when mouse over flyout
        private void SettingFlyout_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #endregion //Event handlers
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
            
            //Detect zoom ??? 2 steps before next image?
            if (e.PreviousSize.Height / e.PreviousSize.Width - e.NewSize.Height / e.NewSize.Width < 0.0001)
            {
                double hZoomTo = Mouse.GetPosition(this).Y / this.ActualHeight; //0.5;
                double wZoomTo = Mouse.GetPosition(this).X / this.ActualWidth;  //0.5;
                // Current view offset, range [0;1]
                double hCVO = (this.VerticalOffset + this.ViewportHeight * hZoomTo) / this.ExtentHeight;
                double wCVO = (this.HorizontalOffset + this.ViewportWidth * wZoomTo) / this.ExtentWidth;

                double hNewOffset = e.NewSize.Height * hCVO - this.ViewportHeight * hZoomTo;
                double wNewOffset = e.NewSize.Width * wCVO - this.ViewportWidth * wZoomTo;

                this.ScrollToVerticalOffset(hNewOffset);
                this.ScrollToHorizontalOffset(wNewOffset);
            }
            else
            {
                this.ScrollToHome();
                //this._ScroolToCenter() ?
            }
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