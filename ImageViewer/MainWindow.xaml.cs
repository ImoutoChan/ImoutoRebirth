using ImageViewer.Model;
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
    public partial class MainWindow : Window
    {
        #region Fields

        private LocalImageList _imageList;
        private ResizeType _currentResizeType = ResizeType.Default;

        #endregion //Fields

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            
            RenderOptions.SetBitmapScalingMode(ViewPort, BitmapScalingMode.Fant);

            ScrollViewerObject.ScrollChanged += ScrollViewerObject_ScrollChanged;

            ImageListInit();
        }

        #endregion //Constructors

        #region Properties

        private LocalImage CurrentImage
        {
            get
            {
                return _imageList.CurrentImage;
            }
        }

        #endregion //Properties

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImageView();
            MarkSelectedItems();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _imageList.Next();
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                _imageList.Previous();
            }
            UpdateImageView();
        }
        
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {

            //horizontal scroll shift — alt+wheel
            if (ScrollViewerObject.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Visible
                && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollViewerObject.ScrollToHorizontalOffset(offset + ScrollViewerObject.HorizontalOffset);
            }
            //zoom — ctrl+wheel
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta < 0)
                {
                    CurrentImage.ZoomOut();
                }
                else
                {                        
                    CurrentImage.ZoomIn();
                }

                double hZoomTo = e.GetPosition(ScrollViewerObject).Y / ScrollViewerObject.ActualHeight; //0.5;
                double wZoomTo = e.GetPosition(ScrollViewerObject).X / ScrollViewerObject.ActualWidth;  //0.5;
                // Current view offset, range [0;1]
                double hCVO = (ScrollViewerObject.VerticalOffset + ScrollViewerObject.ViewportHeight * hZoomTo) / ScrollViewerObject.ExtentHeight;
                double wCVO = (ScrollViewerObject.HorizontalOffset + ScrollViewerObject.ViewportWidth * wZoomTo) / ScrollViewerObject.ExtentWidth;

                double hNewOffset = CurrentImage.ResizedSize.Height * hCVO - ScrollViewerObject.ViewportHeight * hZoomTo;
                double wNewOffset = CurrentImage.ResizedSize.Width * wCVO - ScrollViewerObject.ViewportWidth * wZoomTo;

                ScrollViewerObject.ScrollToVerticalOffset(hNewOffset);
                ScrollViewerObject.ScrollToHorizontalOffset(wNewOffset);
                
                UpdateImageView();
            }
            //vertical scroll shift — shift buttons are up
            else if (ScrollViewerObject.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible
                        && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)))
            {
                double offset = (e.Delta < 0) ? 20 : -20;
                ScrollViewerObject.ScrollToVerticalOffset(offset + ScrollViewerObject.VerticalOffset);
            }
            //next image
            else
            {
                if (e.Delta < 0)
                {
                    _imageList.Next();
                }
                else
                {                        
                    _imageList.Previous();
                }
                UpdateImageView();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateImageView();
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

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Label l = sender as Label;
            switch (l.Name)
            {
                case "LResizeFitVP":
                    _currentResizeType = ResizeType.FitToViewPort;
                    break;
                case "LResizeDownVP":
                    _currentResizeType = ResizeType.DownscaleToViewPort;
                    break;
                case "LResizeFitW":
                    _currentResizeType = ResizeType.FitToViewPortWidth;
                    break;
                case "LResizeDownW":
                    _currentResizeType = ResizeType.DownscaleToViewPortWidth;
                    break;
                case "LResizeNoResize":
                    _currentResizeType = ResizeType.NoResize;
                    break;
                default:
                    _currentResizeType = ResizeType.Default;
                    break;
            }

            MarkSelectedItems();
            UpdateImageView();
        }

        private void ScrollViewerObject_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateWindowPanels();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            #if DEBUG
            if (e.Key == Key.T)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Image current height x width: {0} x {1}.", CurrentImage.ResizedSize.Height, CurrentImage.ResizedSize.Width);
                sb.AppendFormat("\nScrollObject current scrolable height x width: {0} x {1}.", ScrollViewerObject.ScrollableHeight, ScrollViewerObject.ScrollableWidth);
                sb.AppendFormat("\nScrollObject current viewport height x width: {0} x {1}.", ScrollViewerObject.ViewportHeight, ScrollViewerObject.ViewportWidth);
                sb.AppendFormat("\nScrollObject current offset vertical x horisontal: {0} x {1}.", ScrollViewerObject.VerticalOffset, ScrollViewerObject.HorizontalOffset);
                sb.AppendFormat("\nScrollObject current extent height x width: {0} x {1}.", ScrollViewerObject.ExtentHeight, ScrollViewerObject.ExtentWidth);
                sb.AppendFormat("\nScrollObject current actual height x width: {0} x {1}.", ScrollViewerObject.ActualHeight, ScrollViewerObject.ActualWidth);
                sb.AppendFormat("\nScrollObject current height x width: {0} x {1}.", ScrollViewerObject.Height, ScrollViewerObject.Width);

                double hZoomTo = 0.5;
                // Current view offset, range [0;1]
                double hCVO = (ScrollViewerObject.VerticalOffset + ScrollViewerObject.ViewportHeight * hZoomTo) / ScrollViewerObject.ExtentHeight;
                double hNewOffset = ScrollViewerObject.ExtentHeight * hCVO - ScrollViewerObject.ViewportHeight / 2;
                //Correction to range [0; ScrollViewerObject.ScrollableHeight]
                //hNewOffset = (hNewOffset < 0) ? 0 : (hNewOffset > ScrollViewerObject.ScrollableHeight) ? ScrollViewerObject.ScrollableHeight : hNewOffset;

                sb.AppendFormat("\n\nCurrent hCVO, hNewOffset: {0}, {1}.", hCVO, hNewOffset);

                MessageBox.Show(sb.ToString());
            }
            #endif

            if (e.Key == Key.L)
            {
                CurrentImage.RotateLeft();
                UpdateImageView();
            }
            if (e.Key == Key.R)
            {
                CurrentImage.RotateRight();
                UpdateImageView();
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                var imageFiles =
                    from file in droppedFiles
                    where IsImage(file)
                    select file;

                if (imageFiles.Count() == 1)
                {
                    //Load all images from folder
                    FileInfo fi = new FileInfo(imageFiles.First());
                    DirectoryInfo di = fi.Directory;

                    var files =
                        from file in Directory.GetFiles(di.FullName, "*.*")
                        where IsImage(file)
                        select file;

                    _imageList = new LocalImageList(files.ToArray(), imageFiles.First());
                }
                else if (imageFiles.Count() > 0)
                {
                    //Load only dropped images
                    _imageList = new LocalImageList(imageFiles.ToArray());
                }
            }
            UpdateImageView();
        }

        #endregion //Event handlers

        #region Methods

        private void ImageListInit()
        {
            if (Application.Current.Properties["ArbitraryArgName"] != null)
            {
                string fname = Application.Current.Properties["ArbitraryArgName"].ToString();
                //MessageBox.Show(fname);
                FileInfo fi = new FileInfo(fname);
                DirectoryInfo di = fi.Directory;

                var files =
                    from file in Directory.GetFiles(di.FullName, "*.*")
                    where IsImage(file)
                    select file;

                _imageList = new LocalImageList(files.ToArray(), fname);
            }
            #if DEBUG
            else
            {
                var files =
                    from file in Directory.GetFiles((new DirectoryInfo(@"c:\Users\oniii-chan\Downloads\DLS\art\loli\")).FullName, "*.*")
                    where IsImage(file)
                    select file;

                _imageList = new LocalImageList(files.ToArray());
            }
            #endif
        }

        private void UpdateImageView()
        {
            try
            {
                CurrentImage.Resize(new Size(Client.ActualWidth, Client.ActualHeight), _currentResizeType);

                ViewPort.BeginInit();

                ViewPort.Height = CurrentImage.ResizedSize.Height;
                ViewPort.Width = CurrentImage.ResizedSize.Width;

                if (CurrentImage.ImageFormat == ImageFormat.GIF)
                {
                    ViewPort.Source = null;
                    ImageBehavior.SetAnimatedSource(ViewPort, CurrentImage.Image);
                }
                else
                {
                    ImageBehavior.SetAnimatedSource(ViewPort, null);
                    ViewPort.Source = CurrentImage.Image;
                }
                ViewPort.EndInit();

                this.Title = CurrentImage.Name;
            }
            catch {
                throw;
            }
        }

        private void UpdateWindowPanels()
        {
            if (this.ScrollViewerObject.ComputedVerticalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                this.ResizePanel.Margin = new Thickness(0, 0, SystemParameters.VerticalScrollBarWidth, 0);
            }
            else
            {
                this.ResizePanel.Margin = new Thickness(0, 0, 0, 0);
            }
        }

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

        private void MarkSelectedItems()
        {
            Style selectedStyle = this.FindResource("LabelTransButtonSelected") as Style;

            if (_currentResizeType == ResizeType.Default)
            {
                _currentResizeType = LocalImage.DefaultResizeType;
            }

            ResetResizePanelSelected();

            switch (_currentResizeType)
            {
                case ResizeType.FitToViewPort:
                    LResizeFitVP.Style = selectedStyle;
                    break;
                case ResizeType.DownscaleToViewPort:
                    LResizeDownVP.Style = selectedStyle;
                    break;
                case ResizeType.FitToViewPortWidth:
                    LResizeFitW.Style = selectedStyle;
                    break;
                case ResizeType.DownscaleToViewPortWidth:
                    LResizeDownW.Style = selectedStyle;
                    break;
                case ResizeType.NoResize:
                    LResizeNoResize.Style = selectedStyle;
                    break;
                default:
                    break;
            }
        }

        private void ResetResizePanelSelected()
        {
            Style style = this.FindResource("LabelTransButton") as Style;

            LResizeFitVP.Style = style;
            LResizeDownVP.Style = style;
            LResizeFitW.Style = style;
            LResizeDownW.Style = style;
            LResizeNoResize.Style = style;
        }

        #endregion //Methods
    }

    public class ExtScrollViewer : ScrollViewer
    {
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            //Nothing
        }
    }
}