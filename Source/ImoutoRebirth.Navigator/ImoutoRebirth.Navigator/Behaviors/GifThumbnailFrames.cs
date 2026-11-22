using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;
using Imaging = System.Drawing.Imaging;

namespace ImoutoRebirth.Navigator.Behaviors;

internal class GifThumbnailFrames : IDisposable
{
    private const int FrameDelayProperty = 0x5100;
    private const int LoopCountProperty = 0x5101;
    private const int DefaultFrameDelayMs = 100;

    private readonly byte[] _pixels;
    private readonly int[] _delays;
    private readonly RepeatBehavior _repeat;
    private readonly Action<long> _release;
    private long _reservedBytes;

    private GifThumbnailFrames(
        int width,
        int height,
        byte[] pixels,
        int[] delays,
        RepeatBehavior repeat,
        Action<long> release)
    {
        Width = width;
        Height = height;
        _pixels = pixels;
        _delays = delays;
        _repeat = repeat;
        _release = release;
        _reservedBytes = pixels.LongLength;
    }

    public int Width { get; }

    public int Height { get; }

    public static GifThumbnailFrames? TryLoad(
        string path,
        Size viewport,
        Func<long, bool> reserve,
        Action<long> release,
        CancellationToken ct)
    {
        using var stream = File.Open(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read | FileShare.Delete);
        using var gif = Drawing.Image.FromStream(stream);
        var frameCount = gif.GetFrameCount(Imaging.FrameDimension.Time);

        if (frameCount < 2)
            return null;

        var scale = Math.Min(1, Math.Min(viewport.Width / gif.Width, viewport.Height / gif.Height));
        var width = Math.Max(1, (int)Math.Round(gif.Width * scale));
        var height = Math.Max(1, (int)Math.Round(gif.Height * scale));
        var bytes = (long)width * height * 4 * frameCount;

        if (bytes > Array.MaxLength || !reserve(bytes))
            return null;

        var loaded = false;

        try
        {
            var delays = ReadFrameDelays(gif, frameCount);
            var repeat = ReadRepeatBehavior(gif);
            var pixels = RenderFrames(
                gif,
                width,
                height,
                frameCount,
                (int)bytes,
                ct);

            loaded = true;
            return new GifThumbnailFrames(
                width,
                height,
                pixels,
                delays,
                repeat,
                release);
        }
        finally
        {
            if (!loaded)
                release(bytes);
        }
    }

    public AnimationClock CreateClock()
    {
        var timeline = new Int32AnimationUsingKeyFrames
        {
            RepeatBehavior = _repeat
        };

        var elapsed = 0L;

        for (var i = 0; i < _delays.Length; i++)
        {
            timeline.KeyFrames.Add(new DiscreteInt32KeyFrame(i, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(elapsed))));
            elapsed += _delays[i];
        }

        timeline.Duration = new Duration(TimeSpan.FromMilliseconds(elapsed));
        timeline.Freeze();

        return (AnimationClock)timeline.CreateClock(true);
    }

    public void DrawFrame(WriteableBitmap target, int frameIndex)
    {
        if (!target.TryLock(new Duration(TimeSpan.Zero)))
            return;

        try
        {
            var rect = new Int32Rect(0, 0, Width, Height);
            var stride = Width * 4;

            target.WritePixels(
                rect,
                _pixels,
                stride,
                frameIndex * stride * Height);
        }
        finally
        {
            target.Unlock();
        }
    }

    public void Dispose()
    {
        var bytes = Interlocked.Exchange(ref _reservedBytes, 0);

        if (bytes != 0)
            _release(bytes);
    }

    private static byte[] RenderFrames(
        Drawing.Image gif,
        int width,
        int height,
        int frameCount,
        int bytes,
        CancellationToken ct)
    {
        var pixels = GC.AllocateUninitializedArray<byte>(bytes);

        using var thumbnail = new Drawing.Bitmap(width, height, Imaging.PixelFormat.Format32bppPArgb);
        using var graphics = Drawing.Graphics.FromImage(thumbnail);
        graphics.CompositingMode = Drawing.Drawing2D.CompositingMode.SourceCopy;
        graphics.InterpolationMode = Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

        var rect = new Drawing.Rectangle(0, 0, width, height);
        var stride = width * 4;

        for (var i = 0; i < frameCount; i++)
        {
            ct.ThrowIfCancellationRequested();
            gif.SelectActiveFrame(Imaging.FrameDimension.Time, i);
            graphics.DrawImage(
                gif,
                rect,
                0,
                0,
                gif.Width,
                gif.Height,
                Drawing.GraphicsUnit.Pixel);

            var bitmapData = thumbnail.LockBits(
                rect,
                Imaging.ImageLockMode.ReadOnly,
                Imaging.PixelFormat.Format32bppPArgb);

            try
            {
                for (var row = 0; row < height; row++)
                {
                    Marshal.Copy(
                        bitmapData.Scan0 + row * bitmapData.Stride,
                        pixels,
                        (i * height + row) * stride,
                        stride);
                }
            }
            finally
            {
                thumbnail.UnlockBits(bitmapData);
            }
        }

        return pixels;
    }

    private static int[] ReadFrameDelays(Drawing.Image gif, int frameCount)
    {
        var values = ReadProperty(gif, FrameDelayProperty);
        var delays = new int[frameCount];

        for (var i = 0; i < delays.Length; i++)
        {
            var offset = i * sizeof(int);
            var delay = offset <= values.Length - sizeof(int) ? BitConverter.ToInt32(values, offset) : 0;

            delays[i] = delay > 0 && delay <= ushort.MaxValue ? delay * 10 : DefaultFrameDelayMs;
        }

        return delays;
    }

    private static RepeatBehavior ReadRepeatBehavior(Drawing.Image gif)
    {
        var values = ReadProperty(gif, LoopCountProperty);
        if (values.Length < sizeof(ushort))
            return new RepeatBehavior(1);

        var repeats = BitConverter.ToUInt16(values, 0);

        return repeats == 0 ? RepeatBehavior.Forever : new RepeatBehavior(repeats);
    }

    private static byte[] ReadProperty(Drawing.Image gif, int property)
        => gif.PropertyIdList.Contains(property) ? gif.GetPropertyItem(property)?.Value ?? [] : [];
}
