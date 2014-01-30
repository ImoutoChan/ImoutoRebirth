using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ImoutoViewer.UserControls
{
    public class ExtScrollViewer : ScrollViewer
    {
        public bool IsNeedScrollHome { get; set; }

        #region Constructors

        public ExtScrollViewer()
        {
            Loaded += ExtScrollViewer_Loaded;
        }

        #endregion //Constructors

        #region Event handlers

        private void ExtScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var content = Content as FrameworkElement;
            if (content != null)
            {
                content.SizeChanged += ExtScrollViewer_SizeChanged;
            }
        }

        private void ExtScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsNeedScrollHome)
            {
                ScrollToHome();
                IsNeedScrollHome = false;
                return;
            }

            double hZoomTo = Mouse.GetPosition(this).Y/ActualHeight; //0.5;
            double wZoomTo = Mouse.GetPosition(this).X/ActualWidth; //0.5;
            // Current view offset, range [0;1]
            double hCVO = (VerticalOffset + ViewportHeight*hZoomTo)/ExtentHeight;
            double wCVO = (HorizontalOffset + ViewportWidth*wZoomTo)/ExtentWidth;

            double hNewOffset = e.NewSize.Height*hCVO - ViewportHeight*hZoomTo;
            double wNewOffset = e.NewSize.Width*wCVO - ViewportWidth*wZoomTo;

            ScrollToVerticalOffset(hNewOffset);
            ScrollToHorizontalOffset(wNewOffset);
        }

        #endregion //Event handlers

        #region Events

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (ComputedHorizontalScrollBarVisibility == Visibility.Visible
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollToHorizontalOffset(offset + HorizontalOffset);
            }
            else if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                base.OnMouseWheel(e);
            }

            e.Handled = false;
        }

        #endregion //Events
    }
}