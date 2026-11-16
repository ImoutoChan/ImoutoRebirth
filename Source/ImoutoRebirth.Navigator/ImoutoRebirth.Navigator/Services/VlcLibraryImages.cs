using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Serilog;

namespace ImoutoRebirth.Navigator.Services;

internal sealed partial class VlcLibraryImages : IDisposable
{
    private const uint SecImage = 0x01000000;
    private const uint PageReadOnly = 0x02;
    private readonly ConcurrentBag<SafeMemoryMappedFileHandle> _images = new();

    private VlcLibraryImages()
    {
    }

    public static Task<VlcLibraryImages> PrepareAsync(string libraryDirectory) => Task.Run(() =>
    {
        var images = new VlcLibraryImages();
        var watch = Stopwatch.StartNew();

        try
        {
            Parallel.ForEach(
                Directory.EnumerateFiles(libraryDirectory, "*.dll", SearchOption.AllDirectories),
                new ParallelOptions { MaxDegreeOfParallelism = 16 },
                path =>
                {
                    try
                    {
                        using var file = File.OpenHandle(
                            path,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read | FileShare.Delete);
                        var image = CreateFileMapping(file, IntPtr.Zero, SecImage | PageReadOnly, 0, 0, null);
                        if (image.IsInvalid)
                        {
                            var error = Marshal.GetLastPInvokeError();
                            image.Dispose();
                            throw new Win32Exception(error);
                        }

                        images._images.Add(image);
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Unable to prepare VLC library image {Path}", path);
                    }
                });
        }
        catch (Exception e)
        {
            Log.Warning(e, "Unable to prepare VLC library images at {Path}", libraryDirectory);
        }

        Log.Information(
            "Prepared {Count} VLC library images in {ElapsedMilliseconds} ms",
            images._images.Count,
            watch.ElapsedMilliseconds);
        return images;
    });

    public void Dispose()
    {
        while (_images.TryTake(out var image))
            image.Dispose();
    }

    [LibraryImport("kernel32.dll", EntryPoint = "CreateFileMappingW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    private static partial SafeMemoryMappedFileHandle CreateFileMapping(
        SafeFileHandle file,
        IntPtr attributes,
        uint protection,
        uint maximumSizeHigh,
        uint maximumSizeLow,
        string? name);
}