using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Imouto;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;

namespace ImoutoNavigator.ViewModel
{
    class ImageEntryVM : VMBase, INavigatorListEntry
    {
        #region Fields

        private ICommand _openFileCommand;

        #endregion Fields

        #region Constructors

        public ImageEntryVM(string imagePath, Size initPreviewSize = new Size())
        {
            ImageEntry = new ImageEntry(imagePath, initPreviewSize);
            ImageEntry.ImageChanged += (s, e) =>
                                       {
                                           OnPropertyChanged("IsLoading");
                                           OnPropertyChanged("Image");
                                       };

            Type = (ImageEntry.ImageFormat == ImageFormat.GIF) ? ListEntryType.Gif : ListEntryType.Image;
        }

        #endregion Constructors

        #region Properties

        public bool IsLoading => ImageEntry.IsLoading;

        public BitmapSource Image => ImageEntry.Image;

        public Size ViewPortSize => ImageEntry.ViewPort;

        public ListEntryType Type { get; }

        private ImageEntry ImageEntry { get; }

        #endregion Properties

        #region Commands

        public ICommand OpenCommand => _openFileCommand ?? (_openFileCommand = new RelayCommand(Open));

        private void Open(object obj)
        {
            Process.Start(ImageEntry.FullName);
        }

        #endregion Commands

        #region Public methods

        public void UpdatePreview(Size previewSize)
        {
            ImageEntry.UpdatePreview(previewSize);
            OnPropertyChanged("ViewPortSize");
        }

        public void Load()
        {
            ImageEntry.DoLoadAsyns();
        }

        #endregion Public methods

        #region IDragable members

        public object Data => new DataObject(DataFormats.FileDrop, new[] { ImageEntry.FullName });

        public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

        #endregion IDragable members
    }
}
