using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Common;
using Serilog;

namespace ImoutoRebirth.Navigator.UserControls;

/// <summary>
/// Interaction logic for UgoiraPlayer.xaml
/// </summary>
public partial class UgoiraPlayer : UserControl
{
    private bool _isPlaying = false;
    private int _currentFrame = -1;
    private bool _loaded = false;
    private readonly SemaphoreSlim _loadLocker = new(1);

    public UgoiraPlayer()
    {
        InitializeComponent();
    }

    private BitmapSource? CurrentImage => Frames.Count > _currentFrame ? Frames[_currentFrame].Image : null;

    private FrameItem? CurrentFrame => Frames.Count > _currentFrame ? Frames[_currentFrame] : null;

    private List<FrameItem> Frames { get; } = new();
        
    private List<DelayItem> Delays { get; set; } = new();

    public static readonly DependencyProperty SourceProperty 
        = DependencyProperty.Register(
            nameof(Source), 
            typeof (string), 
            typeof (UgoiraPlayer), 
            new UIPropertyMetadata(null, OnSourceChanged));

    public string Source
    {
        get => (string) GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty FrameDelaysProperty 
        = DependencyProperty.Register(
            nameof(FrameDelays), 
            typeof (IEnumerable<DelayItem>), 
            typeof (UgoiraPlayer), 
            new UIPropertyMetadata(null, OnFrameDelaysChanged));

    public IEnumerable<DelayItem>? FrameDelays
    {
        get => (IEnumerable<DelayItem>?) GetValue(FrameDelaysProperty);
        set => SetValue(FrameDelaysProperty, value);
    }

    private static async void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var path = (string) e.NewValue;
        var control = (UgoiraPlayer) d;

        if (string.IsNullOrEmpty(path))
        {
            control.UnloadUgoira();
        }
        else
        {
            control.UnloadUgoira();
            await control.LoadUgoira(path);
        }
    }

    private static void OnFrameDelaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not IEnumerable<DelayItem> delays)
            return;

        var control = (UgoiraPlayer) d;

        control.Delays = delays.ToList();
    }


    private async Task LoadUgoira(string path)
    {
        if (_loaded)
            return;

        await _loadLocker.WaitAsync();

        try
        {
            if (_loaded)
                return;

            Frames.Clear();
            StopPlaying();

            await using var zipToOpen = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (IsImage(entry.Name))
                {
                    await using var previewStream = entry.Open();
                    await using var resultImage = new MemoryStream();
                    await previewStream.CopyToAsync(resultImage);
                    resultImage.Position = 0;

                    var frame = BitmapFrame.Create(resultImage, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                    Frames.Add(new FrameItem(frame, 33, entry.FullName));
                }
                else if (entry.Name.EndsWithIgnoreCase(".json"))
                {
                    try
                    {
                        await using var jsonStream = entry.Open();
                        var data = JsonSerializer.Deserialize<InternalJsonIllustData>(jsonStream)!;
                        FrameDelays = data.Frames?.Select(x => new DelayItem(x.Delay, x.File)) ?? [];
                    }
                    catch (Exception e)
                    {
                        Log.Warning(
                            e,
                            "Failed to load ugoira delay data from internal json {JsonFileName}",
                            entry.Name);
                    }
                }
            }

            StartPlaying();

            _loaded = true;
        }
        finally
        {
            _loadLocker.Release();
        }
    }
    private void UnloadUgoira()
    {
        StopPlaying();
        Frames.Clear();
        _loaded = false;
    }

    private async void StartPlaying()
    {
        if (_isPlaying)
            return;

        _isPlaying = true;
            
        while (_isPlaying)
        {
            var delaysMode = Delays.Any();

            if (!delaysMode)
            {
                _currentFrame++;

                if (!delaysMode && _currentFrame >= Frames.Count)
                    _currentFrame = 0;

                if (Frames.Count == 0)
                    return;

                var current = CurrentFrame;
                var delay = Delays.FirstOrDefault(x => x.FileName == current?.FileName)?.Delay;
                Image.Source = current?.Image;
                await Task.Delay(delay ?? current?.Delay ?? 33);
            }
            else
            {
                _currentFrame++;

                if (_currentFrame >= Delays.Count)
                    _currentFrame = 0;

                if (Frames.Count == 0)
                    return;

                var current = Delays[_currentFrame];
                var frame = Frames.FirstOrDefault(x => x.FileName == current?.FileName);
                Image.Source = frame?.Image;
                await Task.Delay(current.Delay);
            }
        };
    }

    private void StopPlaying()
    {
        _isPlaying = false;
    }

    private static bool IsImage(string path)
    {
        string[] formats = [".jpg", ".png", ".jpeg", ".bmp", ".gif", ".tiff", ".webp", ".jfif"];
        return formats.Any(item => path.EndsWithIgnoreCase(item));
    }
}

public record DelayItem(int Delay, string FileName);

public record FrameItem(BitmapSource Image, int Delay, string FileName);

public record InternalJsonIllustData(
    [property: JsonPropertyName("illustId")] long IllustId,
    [property: JsonPropertyName("userId")] long UserId,
    [property: JsonPropertyName("createDate")] DateTimeOffset CreateDate,
    [property: JsonPropertyName("uploadDate")] DateTimeOffset UploadDate,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("frames")] IReadOnlyCollection<InternalJsonFrame> Frames
);

public record InternalJsonFrame(
    [property: JsonPropertyName("file")] string File,
    [property: JsonPropertyName("delay")] int Delay,
    [property: JsonPropertyName("md5")] string Md5
);
