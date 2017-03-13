using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Imouto.Navigator.Commands;
using Imouto.Navigator.Model;
using Imouto.Utils;

namespace Imouto.Navigator.ViewModel
{
    class ImageEntryVM : VMBase, INavigatorListEntry
    {
        #region Fields

        private ICommand _openFileCommand;

        #endregion Fields

        #region Constructors

        public ImageEntryVM(string imagePath, Size initPreviewSize = new Size(), int? dbId = null)
        {
            ImageEntry = new ImageEntry(imagePath, initPreviewSize);
            ImageEntry.ImageChanged += (s, e) =>
            {
                OnPropertyChanged("IsLoading");
                OnPropertyChanged("Image");
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

        public int? DbId { get; }

        public string Path => ImageEntry.FullName;

        #endregion Properties

        #region Commands

        public ICommand OpenCommand => _openFileCommand ?? (_openFileCommand = new RelayCommand(Open));

        private void Open(object obj)
        {
            try
            {
                var myProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = Associations.AssocQueryString(Associations.AssocStr.ASSOCSTR_EXECUTABLE,
                            "." + ImageEntry.FullName.Split('.').Last()),
                        Arguments = ImageEntry.FullName + $" -nav-guid={App.AppGuid}"
                    }
                };
                myProcess.Start();
            }
            catch (Exception e)
            {
                Process.Start(ImageEntry.FullName);
            }
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

        public object Data => new DataObject(DataFormats.FileDrop, new[]
        {
            ImageEntry.FullName
        });

        public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

        #endregion IDragable members
    }
}
