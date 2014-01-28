using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoViewer.UserControls
{
    public class ExtScrollViewer : ScrollViewer
    {
        public ExtScrollViewer()
        {
            Loaded += ExtScrollViewer_Loaded;            
        }

        private void ExtScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var content = Content as FrameworkElement;
            if (content != null)
                content.SizeChanged += ExtScrollViewer_SizeChanged;
        }

        private void ExtScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Detect zoom ??? 2 steps before next image?
            if (e.PreviousSize.Height / e.PreviousSize.Width - e.NewSize.Height / e.NewSize.Width < 0.0001)
            {
                double hZoomTo = Mouse.GetPosition(this).Y / ActualHeight; //0.5;
                double wZoomTo = Mouse.GetPosition(this).X / ActualWidth;  //0.5;
                // Current view offset, range [0;1]
                double hCVO = (VerticalOffset + ViewportHeight * hZoomTo) / ExtentHeight;
                double wCVO = (HorizontalOffset + ViewportWidth * wZoomTo) / ExtentWidth;

                double hNewOffset = e.NewSize.Height * hCVO - ViewportHeight * hZoomTo;
                double wNewOffset = e.NewSize.Width * wCVO - ViewportWidth * wZoomTo;

                ScrollToVerticalOffset(hNewOffset);
                ScrollToHorizontalOffset(wNewOffset);
            }
            else
            {
                ScrollToHome();
                //this._ScroolToCenter() ?
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollToHorizontalOffset(offset + HorizontalOffset);
            }
            else
            {
                base.OnMouseWheel(e);
            }

            e.Handled = false;
        }
    }
}