using System.Windows;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal class ImageEntryVM : BaseEntryVM, INavigatorListEntry, IPixelSizable
{
    private bool _gifPreviewVisible;
    private bool _isGifPlaybackSuspended;

    public ImageEntryVM(string imagePath, Size initPreviewSize, Guid? dbId)
        : base(dbId)
    {
        ImageEntry = new ImageEntry(imagePath, initPreviewSize);
        
        ImageEntry.ImageChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(GifSourceUri));
            OnPropertyChanged(nameof(PixelSize));
        };

        Type = ImageEntry.ImageFormat switch
        {
            ImageFormat.GIF => ListEntryType.Gif,
            ImageFormat.PNG => ListEntryType.Png,
            ImageFormat.WEBP => ListEntryType.WebP,
            _ => ListEntryType.Image
        };
    }

    public bool IsLoading => ImageEntry.IsLoading;

    public BitmapSource? Image => ImageEntry.Image;

    public Uri? GifSourceUri => _gifPreviewVisible && Type == ListEntryType.Gif && Image is BitmapImage bitmap
        ? bitmap.UriSource
        : null;

    public bool IsGifPlaybackSuspended
    {
        get => _isGifPlaybackSuspended;
        set => SetProperty(ref _isGifPlaybackSuspended, value);
    }

    public Size ViewPortSize => ImageEntry.ViewPort;
    
    public Size? PixelSize => ImageEntry.ImageSize;

    public ListEntryType Type { get; private set; }

    private ImageEntry ImageEntry { get; }

    public string Path => ImageEntry.FullName;

    public void SetGifPreviewVisible(bool isVisible)
    {
        if (_gifPreviewVisible == isVisible)
            return;

        _gifPreviewVisible = isVisible;
        OnPropertyChanged(nameof(GifSourceUri));
    }

    public void UpdatePreview(Size previewSize)
    {
        ImageEntry.UpdatePreview(previewSize);
        OnPropertyChanged(nameof(ViewPortSize));
    }

    public void Load() => ImageEntry.DoLoadAsync();

    public object Data => new DataObject(DataFormats.FileDrop, new[] { ImageEntry.FullName });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;
}
