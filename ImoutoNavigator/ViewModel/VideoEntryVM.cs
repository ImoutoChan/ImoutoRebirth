using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Imouto.Navigator.Commands;
using Microsoft.WindowsAPICodePack.Shell;
using System.IO;

namespace Imouto.Navigator.ViewModel
{
    class VideoEntryVM : VMBase, INavigatorListEntry
    {
        #region Fields

        private ICommand _openFileCommand;
        private Size _size;
        private bool _isLoading;
        private bool _isPreviewLoaded;
        private readonly object _loaderLocker = new object();

        #endregion Fields

        #region Constructors

        public VideoEntryVM(string path, Size initPreviewSize = new Size(), int? dbId = null)
        {
            if (!path.IsVideo())
            {
                throw new ArgumentException("Unsupported video format.");
            }
            else if (!(new FileInfo(path)).Exists)
            {
                throw new ArgumentException($"File {path} doesn't exist.");
            }
            else
            {
                Path = path;
                Type = ListEntryType.Video;
                ViewPortSize = initPreviewSize;
            }
            DbId = dbId;
        }

        #endregion Constructors

        #region Properties

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            private set
            {
                OnPropertyChanged(ref _isLoading, value, () => IsLoading);
            }
        }

        public BitmapSource Image { get; private set; }

        public Size ViewPortSize
        {
            get
            {
                return _size;
            }
            private set
            {
                _size = value;
                OnPropertyChanged(() => ViewPortSize);
            }
        }

        public string Path { get; }

        public ListEntryType Type { get; }

        public int? DbId { get; }

        #endregion Properties

        #region Commands

        public ICommand OpenCommand => _openFileCommand ?? (_openFileCommand = new RelayCommand(Open));

        private void Open(object obj)
        {
            Process.Start(Path);
        }

        #endregion Commands

        #region Public methods

        public void UpdatePreview(Size previewSize)
        {
            ViewPortSize = previewSize;
        }

        public void Load()
        {
            LoadPreview();
        }

        #endregion Public methods

        #region Private methods

        private async void LoadPreview()
        {
            lock (_loaderLocker)
            {
                if (_isPreviewLoaded || IsLoading)
                {
                    return;
                }

                IsLoading = true;
            }

            var thumbNail = await Task.Run(() =>
            {
                BitmapSource result;
                using (var shellFile = ShellFile.FromFilePath(Path))
                {
                    result = shellFile.Thumbnail.BitmapSource;
                    result.Freeze();
                }
                return result;
            });
            Image = thumbNail;


            _isPreviewLoaded = true;
            IsLoading = false;
            OnPropertyChanged(() => Image);
        }

        #endregion Private methods

        #region IDragable members

        public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });

        public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

        #endregion IDragable members
    }
}