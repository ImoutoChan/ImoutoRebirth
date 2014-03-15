using System;
using System.Windows;
using ImoutoNavigator.Model;
using System.Windows.Media.Imaging;

namespace ImoutoNavigator.ViewModel
{
    class ImageEntryVM : VMBase
    {
        #region Fields

        private readonly ImageEntry _imageEntry;

        #endregion //Fields

        #region Constructors

        public ImageEntryVM(string path, Size initPreviewSize = new Size())
        {
            _imageEntry = new ImageEntry(path, initPreviewSize);
            _imageEntry.ImageChanged += (s, e) => 
                {
                    OnPropertyChanged("IsLoading");
                    OnPropertyChanged("Image");
                    OnImageChanged(); 
                };
        }

        #endregion //Constructors

        #region Properties

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
    }
}
