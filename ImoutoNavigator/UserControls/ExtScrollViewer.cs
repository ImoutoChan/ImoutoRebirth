using System.Windows.Controls;
using System.Windows.Input;

namespace Imouto.Navigator.UserControls
{
    public class ExtScrollViewer : ScrollViewer
    {
        //public bool IsNeedScrollHome { get; set; }

        #region Constructors

        public ExtScrollViewer()
        {
        }

        #endregion //Constructors

        //#region Event handlers

        //private void ExtScrollViewer_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var content = Content as FrameworkElement;
        //    if (content != null)
        //    {
        //        content.SizeChanged += ExtScrollViewer_SizeChanged;
        //    }
        //}

        //private void ExtScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    if (IsNeedScrollHome)
        //    {
        //        ScrollToHome();
        //        IsNeedScrollHome = false;
        //        return;
        //    }

        //    double hZoomTo = Mouse.GetPosition(this).Y / ActualHeight; //0.5;
        //    double wZoomTo = Mouse.GetPosition(this).X / ActualWidth; //0.5;
        //    // Current view offset, range [0;1]
        //    double hCVO = (VerticalOffset + ViewportHeight * hZoomTo) / ExtentHeight;
        //    double wCVO = (HorizontalOffset + ViewportWidth * wZoomTo) / ExtentWidth;

        //    double hNewOffset = e.NewSize.Height * hCVO - ViewportHeight * hZoomTo;
        //    double wNewOffset = e.NewSize.Width * wCVO - ViewportWidth * wZoomTo;

        //    ScrollToVerticalOffset(hNewOffset);
        //    ScrollToHorizontalOffset(wNewOffset);
        //}

        //#endregion //Event handlers

        #region Methods

        private void Scrolling(int delta)
        {
            //base.OnMouseWheel(e);
            
            //if (this.HandlesMouseWheelScrolling)
            {
                if (this.ScrollInfo != null)
                {
                    if (delta < 0)
                    {
                        this.ScrollInfo.LineDown();
                    }
                    else
                    {
                        this.ScrollInfo.LineUp();
                    }
                }
            }
        }

        #endregion Methods

        #region Events

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && !e.Handled)
            {
                Scrolling(e.Delta);
                e.Handled = true;
                return;
            }

            e.Handled = false;
        }

        #endregion //Events
    }
}