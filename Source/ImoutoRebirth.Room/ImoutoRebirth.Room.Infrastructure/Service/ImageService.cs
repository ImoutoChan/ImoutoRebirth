using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using SixLabors.ImageSharp;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class ImageService : IImageService
{
    public bool IsImageHasSupportedExtension(SystemFile file)
        => SupportedExtensions.Contains(file.File.Extension.ToLowerInvariant());

    public bool IsImageCorrect(SystemFile file)
    {
        try
        {
            using var image = Image.Load(file.File.FullName);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static readonly string[] SupportedExtensions =
    {
        ".png", ".apng", ".ppm", ".pbm", ".pgm",
        ".tga", ".vda", ".icb", ".vst", ".webp",
        ".cur", ".bmp", ".dip", ".bm", ".jpg",
        ".jpeg", ".jfif", ".tiff", ".tif", ".ico",
        ".gif", ".qoi"
    };
}
