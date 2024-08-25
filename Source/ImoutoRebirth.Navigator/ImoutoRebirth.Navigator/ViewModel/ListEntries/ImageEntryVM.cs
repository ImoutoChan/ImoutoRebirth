using System.Windows;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Model;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal class ImageEntryVM : BaseEntryVM, INavigatorListEntry, IPixelSizable
{
    public ImageEntryVM(string imagePath, FilesClient filesClient, Size initPreviewSize, Guid? dbId)
        : base(dbId, filesClient)
    {
        ImageEntry = new ImageEntry(imagePath, initPreviewSize);
        
        ImageEntry.ImageChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(Image));
        };

        Type = ImageEntry.ImageFormat switch
        {
            ImageFormat.GIF => ListEntryType.Gif,
            ImageFormat.PNG => ListEntryType.Png,
            ImageFormat.WEBP => ListEntryType.WebP,
            _ => ListEntryType.Image
        };

        DbId = dbId;
    }

    public bool IsLoading => ImageEntry.IsLoading;

    public BitmapSource? Image => ImageEntry.Image;

    public Size ViewPortSize => ImageEntry.ViewPort;
    
    public Size? PixelSize => ImageEntry.ImageSize;

    public ListEntryType Type { get; private set; }

    private ImageEntry ImageEntry { get; }

    public Guid? DbId { get; }

    public string Path => ImageEntry.FullName;

    public void UpdatePreview(Size previewSize)
    {
        ImageEntry.UpdatePreview(previewSize);
        OnPropertyChanged(nameof(ViewPortSize));
    }

    public async void Load()
    {
        ImageEntry.DoLoadAsync();
        await LoadRating();
    }

    public object Data => new DataObject(DataFormats.FileDrop, new[] { ImageEntry.FullName });

    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;
}
