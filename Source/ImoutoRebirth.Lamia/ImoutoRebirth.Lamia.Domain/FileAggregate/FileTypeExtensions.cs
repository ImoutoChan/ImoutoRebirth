using ImoutoRebirth.Common;

namespace ImoutoRebirth.Lamia.Domain.FileAggregate;

public static class FileTypeExtensions
{
    private static readonly string[] ImageExtensions =
    [
        "png", "apng", "ppm", "pbm", "pgm",
        "tga", "vda", "icb", "vst", "webp",
        "cur", "bmp", "dip", "bm", "jpg",
        "jpeg", "jfif", "tiff", "tif", "ico",
        "gif", "qoi"
    ];

    private static readonly string[] VideoExtensions =
    [
        "mp4", "webm", "wmv", "mov", "flv",
        "avi", "mkv", "m4v", "mpg", "f4v",
        "mpeg", "swf", "m2ts", "m4a"
    ];

    private static readonly string[] ArchiveExtensions =
    [
        "cbz", "zip", "cbr", "cb7"
    ];

    public static bool IsImage(this string fileFullName)
    {
        var extension = Path.GetExtension(fileFullName).Trim('.');
        return ImageExtensions.ContainsIgnoreCase(extension);
    }

    public static bool IsVideo(this string fileFullName)
    {
        var extension = Path.GetExtension(fileFullName).Trim('.');
        return VideoExtensions.ContainsIgnoreCase(extension);
    }

    public static bool IsArchive(this string fileFullName)
    {
        var extension = Path.GetExtension(fileFullName).Trim('.');
        return ArchiveExtensions.ContainsIgnoreCase(extension);
    }
}
