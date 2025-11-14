using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Lilin.WebApi.Client;

namespace ImoutoRebirth.Navigator.ViewModel.ListEntries;

internal partial class UgoiraEntryVM : BaseEntryVM, INavigatorListEntry, IPixelSizable
{
    [ObservableProperty]
    public partial Size ViewPortSize { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    private BitmapSource? _image;

    public UgoiraEntryVM(
        string path,
        FilesClient filesClient,
        Size viewPortSize,
        Guid? dbId) 
        : base(dbId, filesClient)
    {
        DbId = dbId;
        ViewPortSize = viewPortSize;
        Path = path;
    }

    public ListEntryType Type => ListEntryType.Ugoira;

    public Guid? DbId { get; }

    public string Path { get; }

    public Size? PixelSize => Image != null ? new Size(Image.PixelWidth, Image.PixelHeight) : null;

    public object Data => new DataObject(DataFormats.FileDrop, new[] { Path });
        
    public DragDropEffects AllowDragDropEffects => DragDropEffects.Copy;

    public async void Load()
    {
        IsLoading = true;
            
        var ratingTask = LoadRating();

        await using var archive = await ZipFile.OpenReadAsync(Path);
        var preview = archive.Entries.FirstOrDefault();

        if (preview == null)
            return;

        await using var previewStream = await preview.OpenAsync();
        await using var resultImage = new MemoryStream();
        await previewStream.CopyToAsync(resultImage);
        resultImage.Position = 0;

        Image = BitmapFrame.Create(resultImage, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

        IsLoading = false;

        await ratingTask;
    }

    public void UpdatePreview(Size previewSize)
    {
        ViewPortSize = previewSize;
    }
}
