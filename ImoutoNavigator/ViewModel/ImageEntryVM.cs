using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImoutoNavigator.Behavior;
using ImoutoNavigator.Commands;
using ImoutoNavigator.Model;

namespace ImoutoNavigator.ViewModel
{
    class ImageEntryVM : VMBase, IDragable
    {
        #region Fields

        //private ImageM _imageModel;
        private readonly ImageEntry _imageEntry;
        private ICommand _openFileCommand;

        #endregion //Fields

        #region Constructors

        //public ImageEntryVM(ImageM imageModel, Size initPreviewSize = new Size())
        //{
        //    _imageModel = imageModel;

        //    _imageEntry = new ImageEntry(_imageModel.Path, initPreviewSize);
        //    _imageEntry.ImageChanged += (s, e) => 
        //        {
        //            OnPropertyChanged("IsLoading");
        //            OnPropertyChanged("Image");
        //            OnImageChanged(); 
        //        };
        //}

        public ImageEntryVM(string imagePath, Size initPreviewSize = new Size())
        {
            _imageEntry = new ImageEntry(imagePath, initPreviewSize);
            _imageEntry.ImageChanged += (s, e) =>
            {
                OnPropertyChanged("IsLoading");
                OnPropertyChanged("Image");
                OnImageChanged();
            };
        }

        #endregion //Constructors

        #region Properties

        //public ImageM ImageModel
        //{
        //    get
        //    {
        //        return _imageModel;
        //    }
        //    set
        //    {
        //        _imageModel = value;
        //    }
        //}

        public ImageEntry ImageEntry
        {
            get
            {
                return _imageEntry;
            }
        }

        public bool IsLoading
        {
            get { return _imageEntry.IsLoading; } 
        }

        public BitmapSource Image
        {
            get { return _imageEntry.Image; }
        }

        public Size ViewPortSize
        {
            get { return _imageEntry.ViewPort; }
        }

        #endregion //Properties

        #region Commands

        public ICommand OpenCommand
        {
            get
            {
                return _openFileCommand ?? (_openFileCommand = new RelayCommand(Open));
            }
        }

        private void Open(object obj)
        {
            Process.Start(_imageEntry.FullName);
        }

        #endregion //Commands

        #region Methods

        #endregion //Methods

        #region Public methods
        
        public void UpdatePreview(Size previewSize)
        {
            _imageEntry.UpdatePreview(previewSize);
            OnPropertyChanged("ViewPortSize");
        }

        public void Load()
        {
            _imageEntry.DoLoadAsyns();
        }

        #endregion //Public methods

        #region Events

        public event EventHandler ImageEntryChanged;

        private void OnImageChanged()
        {
            if (ImageEntryChanged != null)
            {
                ImageEntryChanged(this, new EventArgs());
            }
        }

        #endregion //Events

        #region Event handlers

        #endregion //Event handlers

        #region IDragable members

        public object Data
        {
            get
            {
                var data = new DataObject(DataFormats.FileDrop, new[] { ImageEntry.FullName });
                //data.SetData("DragSource", _appGuid);
                return data;
            }
        }

        public DragDropEffects AllowDragDropEffects
        {
            get
            {
                return DragDropEffects.Copy;
            }
        }

        #endregion IDragable members
    }
}
