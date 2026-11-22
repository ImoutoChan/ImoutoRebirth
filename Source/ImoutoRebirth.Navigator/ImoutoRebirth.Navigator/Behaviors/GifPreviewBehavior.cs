using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ImoutoRebirth.Common.WPF;
using Microsoft.Xaml.Behaviors;
using Serilog;
using XamlAnimatedGif;

namespace ImoutoRebirth.Navigator.Behaviors;

internal partial class GifPreviewBehavior : Behavior<Image>
{
    private const int ReadBufferSize = 256 * 1024;
    private const long MaxGalleryFrameCacheBytes = 2L * 1024 * 1024 * 1024;
    private const uint AnimationTimerResolution = 1;
    private static readonly SemaphoreSlim ThumbnailDecoders = new(4);

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source),
        typeof(Uri),
        typeof(GifPreviewBehavior),
        new PropertyMetadata(null, (d, _) => ((GifPreviewBehavior)d).Refresh()));

    public static readonly DependencyProperty IsPlaybackSuspendedProperty = DependencyProperty.Register(
        nameof(IsPlaybackSuspended),
        typeof(bool),
        typeof(GifPreviewBehavior),
        new PropertyMetadata(false, (d, _) => ((GifPreviewBehavior)d).UpdatePlayback()));

    public static readonly DependencyProperty ViewportAnchorProperty = DependencyProperty.Register(
        nameof(ViewportAnchor),
        typeof(FrameworkElement),
        typeof(GifPreviewBehavior),
        new PropertyMetadata(null, OnViewportAnchorChanged));

    private static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register(
        "FrameIndex",
        typeof(int),
        typeof(GifPreviewBehavior),
        new PropertyMetadata(0, (d, e) => ((GifPreviewBehavior)d).DrawThumbnailFrame((int)e.NewValue)));

    private static long _galleryFrameCacheBytes;

    private CancellationTokenSource? _loading;
    private Stream? _stream;
    private long _frameCacheBytes;
    private Animator? _playingAnimation;
    private bool _usesPreciseTimer;
    private ScrollContentPresenter? _viewport;
    private GifThumbnailFrames? _thumbnail;
    private WriteableBitmap? _thumbnailBitmap;
    private AnimationClock? _thumbnailClock;

    public Uri? Source
    {
        get => (Uri?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public bool IsPlaybackSuspended
    {
        get => (bool)GetValue(IsPlaybackSuspendedProperty);
        set => SetValue(IsPlaybackSuspendedProperty, value);
    }

    public FrameworkElement? ViewportAnchor
    {
        get => (FrameworkElement?)GetValue(ViewportAnchorProperty);
        set => SetValue(ViewportAnchorProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.Loaded += OnLoaded;
        AssociatedObject.Unloaded += OnUnloaded;
        AssociatedObject.IsVisibleChanged += OnVisibilityChanged;
        AnimationBehavior.AddAnimationStartedHandler(AssociatedObject, OnAnimationStarted);

        if (ViewportAnchor is { } anchor)
            anchor.SizeChanged += OnAnchorSizeChanged;

        if (AssociatedObject.IsLoaded)
            UpdateViewport();
    }

    protected override void OnDetaching()
    {
        AssociatedObject.Loaded -= OnLoaded;
        AssociatedObject.Unloaded -= OnUnloaded;
        AssociatedObject.IsVisibleChanged -= OnVisibilityChanged;
        AnimationBehavior.RemoveAnimationStartedHandler(AssociatedObject, OnAnimationStarted);

        if (ViewportAnchor is { } anchor)
            anchor.SizeChanged -= OnAnchorSizeChanged;

        _viewport = null;
        Stop();

        base.OnDetaching();
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => UpdateViewport();

    private void UpdateViewport()
    {
        _viewport = AssociatedObject.GetParentOfType<ScrollContentPresenter>();
        Refresh();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _viewport = null;
        Stop();
    }

    private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) => Refresh();

    private static void OnViewportAnchorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (GifPreviewBehavior)d;
        if (behavior.AssociatedObject is null)
            return;

        if (e.OldValue is FrameworkElement previous)
            previous.SizeChanged -= behavior.OnAnchorSizeChanged;

        if (e.NewValue is FrameworkElement next)
            next.SizeChanged += behavior.OnAnchorSizeChanged;

        behavior.UpdatePlayback();
    }

    private void OnAnchorSizeChanged(object sender, SizeChangedEventArgs e) => UpdatePlayback();

    private bool IsInsideViewport()
    {
        if (_viewport is null)
            return true;

        if (ViewportAnchor is not { } anchor || !_viewport.IsAncestorOf(anchor))
            return false;

        var viewportBounds = new Rect(_viewport.RenderSize);
        var anchorBounds = anchor.TransformToAncestor(_viewport).TransformBounds(new Rect(anchor.RenderSize));
        var visibleBounds = Rect.Intersect(viewportBounds, anchorBounds);

        return !visibleBounds.IsEmpty && visibleBounds.Width > 0 && visibleBounds.Height > 0;
    }

    private void UpdatePlayback()
    {
        if (!IsPlaybackSuspended && !IsInsideViewport())
        {
            Stop();
            return;
        }

        if (_thumbnailClock is { } clock)
        {
            if (IsPlaybackSuspended)
            {
                clock.Controller?.Pause();
            }
            else if (clock.IsPaused)
            {
                clock.Controller?.Resume();
            }

            return;
        }

        if (_playingAnimation is not { } animator)
        {
            if (_stream is null && _loading is null
                || IsPlaybackSuspended && AnimationBehavior.GetAnimator(AssociatedObject) is null)
                Refresh();

            return;
        }

        if (IsPlaybackSuspended)
        {
            animator.Pause();
            ReleasePreciseTiming();
        }
        else if (animator.IsPaused)
        {
            RequestPreciseTiming();
            animator.Play();
        }
    }

    private async void Refresh()
    {
        Stop();

        if (AssociatedObject is not { IsLoaded: true, IsVisible: true } image
            || Source is not { IsAbsoluteUri: true, IsFile: true } source
            || IsPlaybackSuspended)
            return;

        using var loading = new CancellationTokenSource();
        var ct = loading.Token;
        _loading = loading;

        try
        {
            await System.Windows.Threading.Dispatcher.Yield(DispatcherPriority.Loaded);

            if (ct.IsCancellationRequested || !IsInsideViewport())
                return;

            var anchor = ViewportAnchor ?? image;
            var size = anchor.RenderSize;
            var dpi = VisualTreeHelper.GetDpi(anchor);
            var pixels = new Size(size.Width * dpi.DpiScaleX, size.Height * dpi.DpiScaleY);

            var thumbnail = await CreateThumbnail(source.LocalPath, pixels, ct);
            if (thumbnail is not null)
            {
                if (ct.IsCancellationRequested || !IsInsideViewport())
                {
                    thumbnail.Dispose();
                }
                else
                {
                    StartThumbnail(image, thumbnail);
                }

                return;
            }

            if (ct.IsCancellationRequested || !IsInsideViewport())
                return;

            var (stream, frameBytes) = await Task.Run(() => OpenPreview(source.LocalPath, ct), ct);

            if (ct.IsCancellationRequested || !IsInsideViewport())
            {
                await ClosePreviewStream(stream);
                return;
            }

            _stream = stream;
            _frameCacheBytes = ReserveFrameCache(frameBytes);

            AnimationBehavior.SetCacheFramesInMemory(image, _frameCacheBytes > 0);
            AnimationBehavior.SetSourceStream(image, stream);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Log.Debug(e, "Could not open GIF preview at {Path}", source.LocalPath);
        }
        finally
        {
            if (ReferenceEquals(_loading, loading))
                _loading = null;
        }
    }

    private void Stop()
    {
        _loading?.Cancel();
        _loading = null;
        DetachPlayback();
        StopThumbnail();

        if (_stream is not { } stream)
            return;

        _stream = null;

        AnimationBehavior.SetSourceStream(AssociatedObject, null);
        _ = ClosePreviewStream(stream);

        Interlocked.Add(ref _galleryFrameCacheBytes, -_frameCacheBytes);
        _frameCacheBytes = 0;
    }

    private static async Task<GifThumbnailFrames?> CreateThumbnail(
        string path,
        Size pixels,
        CancellationToken ct)
    {
        if (pixels.Width <= 0 || pixels.Height <= 0)
            return null;

        await ThumbnailDecoders.WaitAsync(ct).ConfigureAwait(false);

        try
        {
            return await Task.Run(
                    () => GifThumbnailFrames.TryLoad(
                        path,
                        pixels,
                        bytes => ReserveFrameCache(bytes) != 0,
                        bytes => Interlocked.Add(ref _galleryFrameCacheBytes, -bytes),
                        ct),
                    ct)
                .ConfigureAwait(false);
        }
        catch (Exception e) when (e is ArgumentException or ExternalException)
        {
            Log.Debug(e, "Using buffered playback for GIF preview at {Path}", path);
            return null;
        }
        finally
        {
            ThumbnailDecoders.Release();
        }
    }

    private void StartThumbnail(Image image, GifThumbnailFrames thumbnail)
    {
        _thumbnail = thumbnail;
        _thumbnailBitmap = new WriteableBitmap(
            thumbnail.Width,
            thumbnail.Height,
            96,
            96,
            PixelFormats.Pbgra32,
            null);

        image.Source = _thumbnailBitmap;
        DrawThumbnailFrame(0);

        _thumbnailClock = thumbnail.CreateClock();
        ApplyAnimationClock(FrameIndexProperty, _thumbnailClock);
    }

    private void DrawThumbnailFrame(int index)
    {
        if (_thumbnail is { } thumbnail && _thumbnailBitmap is { } bitmap)
            thumbnail.DrawFrame(bitmap, index);
    }

    private void StopThumbnail()
    {
        if (_thumbnail is not { } thumbnail)
            return;

        _thumbnail = null;
        _thumbnailBitmap = null;

        _thumbnailClock?.Controller?.Remove();
        ApplyAnimationClock(FrameIndexProperty, null);
        _thumbnailClock = null;

        AssociatedObject.Source = null;
        thumbnail.Dispose();
    }

    private void OnAnimationStarted(object? sender, AnimationStartedEventArgs e)
    {
        if (_playingAnimation is not null || AnimationBehavior.GetAnimator(AssociatedObject) is not { FrameCount: > 1 } animator)
            return;

        _playingAnimation = animator;
        animator.AnimationCompleted += OnAnimationCompleted;
        animator.Error += OnAnimationError;
        RequestPreciseTiming();
    }

    private void OnAnimationCompleted(object? sender, AnimationCompletedEventArgs e) => DetachPlayback();

    private void OnAnimationError(object? sender, AnimationErrorEventArgs e) => DetachPlayback();

    private void DetachPlayback()
    {
        if (_playingAnimation is not { } animator)
            return;

        _playingAnimation = null;
        animator.AnimationCompleted -= OnAnimationCompleted;
        animator.Error -= OnAnimationError;
        ReleasePreciseTiming();
    }

    private void RequestPreciseTiming()
    {
        if (!_usesPreciseTimer)
            _usesPreciseTimer = TimeBeginPeriod(AnimationTimerResolution) == 0;
    }

    private void ReleasePreciseTiming()
    {
        if (!_usesPreciseTimer)
            return;

        _usesPreciseTimer = false;
        _ = TimeEndPeriod(AnimationTimerResolution);
    }

    private static async Task ClosePreviewStream(Stream stream)
    {
        try
        {
            await stream.DisposeAsync().ConfigureAwait(false);
        }
        catch (IOException e)
        {
            Log.Debug(e, "Could not close the GIF preview stream");
        }
    }

    private static (Stream Stream, long FrameBytes) OpenPreview(string path, CancellationToken ct)
    {
        var frameBytes = MeasureFrameCache(path, ct);
        ct.ThrowIfCancellationRequested();

        var stream = new FileStream(path, new FileStreamOptions
        {
            Access = FileAccess.Read,
            Mode = FileMode.Open,
            Share = FileShare.Read | FileShare.Delete,
            Options = FileOptions.Asynchronous,
            BufferSize = 0
        });

        return (new BufferedStream(stream, ReadBufferSize), frameBytes);
    }

    private static long MeasureFrameCache(string path, CancellationToken ct)
    {
        try
        {
            using var stream = File.Open(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read | FileShare.Delete);

            var decoder = new GifBitmapDecoder(
                stream,
                BitmapCreateOptions.None,
                BitmapCacheOption.None);

            var bytes = 0L;

            foreach (var frame in decoder.Frames)
            {
                ct.ThrowIfCancellationRequested();
                bytes += (long)frame.PixelWidth * frame.PixelHeight;

                if (bytes > MaxGalleryFrameCacheBytes)
                    return 0;
            }

            return bytes;
        }
        catch (Exception e) when (e is FileFormatException or NotSupportedException)
        {
            return 0;
        }
    }

    private static long ReserveFrameCache(long bytes)
    {
        if (bytes == 0)
            return 0;

        if (Interlocked.Add(ref _galleryFrameCacheBytes, bytes) <= MaxGalleryFrameCacheBytes)
            return bytes;

        Interlocked.Add(ref _galleryFrameCacheBytes, -bytes);
        return 0;
    }

    [LibraryImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static partial uint TimeBeginPeriod(uint period);

    [LibraryImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static partial uint TimeEndPeriod(uint period);
}
