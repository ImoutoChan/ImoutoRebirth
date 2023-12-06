using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Domain;
using SixLabors.ImageSharp;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class ImageService : IImageService
{
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
}
