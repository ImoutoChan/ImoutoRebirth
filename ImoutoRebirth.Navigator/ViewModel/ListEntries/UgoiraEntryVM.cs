using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries
{
    internal class UgoiraEntryVM : VMBase, INavigatorListEntry
    {
        private Size _size;
        private bool _isLoading;

        public UgoiraEntryVM(string path, Size viewPortSize, Guid? dbId)
        {
            DbId = dbId;
            ViewPortSize = viewPortSize;
            Path = path;
        }

        public ListEntryType Type => ListEntryType.Ugoira;
        
        public Size ViewPortSize
        {
            get => _size;
            private set => OnPropertyChanged(ref _size, value, () => ViewPortSize);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            private set => OnPropertyChanged(ref _isLoading, value, () => IsLoading);
        }

        public Guid? DbId { get; }

        public string Path { get; }

        public BitmapSource? Image { get; private set; }
        
        public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });
        
        public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

        public async void Load()
        {
            IsLoading = true;

            await using var zipToOpen = new FileStream(Path, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

            var preview = archive.Entries.FirstOrDefault();

            if (preview == null)
                return;

            await using var previewStream = preview.Open();
            await using var resultImage = new MemoryStream();
            await previewStream.CopyToAsync(resultImage);
            resultImage.Position = 0;

            Image = BitmapFrame.Create(resultImage, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            OnPropertyChanged(() => Image);

            IsLoading = false;
        }

        public void UpdatePreview(Size previewSize)
        {
            ViewPortSize = previewSize;
        }
    }
}
