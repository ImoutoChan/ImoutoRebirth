using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Imouto;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries
{
    internal class ImageEntryVM : VMBase, INavigatorListEntry
    {
        #region Constructors

        public ImageEntryVM(string imagePath, Size initPreviewSize = new Size(), Guid? dbId = null)
        {
            ImageEntry = new ImageEntry(imagePath, initPreviewSize);
            ImageEntry.ImageChanged += (s, e) =>
            {
                OnPropertyChanged(() => IsLoading);
                OnPropertyChanged(() => Image);
            };

            Type = (ImageEntry.ImageFormat == ImageFormat.GIF)
                   ? ListEntryType.Gif
                   : ListEntryType.Image;
            DbId = dbId;
        }

        #endregion Constructors

        #region Properties

        public bool IsLoading => ImageEntry.IsLoading;

        public BitmapSource Image => ImageEntry.Image;

        public Size ViewPortSize => ImageEntry.ViewPort;

        public ListEntryType Type { get; }

        private ImageEntry ImageEntry { get; }

        public Guid? DbId { get; }

        public string Path => ImageEntry.FullName;

        #endregion Properties

        #region Commands

        #endregion Commands

        #region Public methods

        public void UpdatePreview(Size previewSize)
        {
            ImageEntry.UpdatePreview(previewSize);
            OnPropertyChanged(() => ViewPortSize);
        }

        public void Load()
        {
            ImageEntry.DoLoadAsyns();
        }

        #endregion Public methods

        #region IDragable members

        public object Data => new DataObject(DataFormats.FileDrop, new[]
        {
            ImageEntry.FullName
        });

        public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

        #endregion IDragable members
    }
}
