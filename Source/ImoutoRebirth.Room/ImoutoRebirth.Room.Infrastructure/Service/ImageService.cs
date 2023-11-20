using ImoutoRebirth.Room.Application;
using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class ImageService : IImageService
{
    public bool IsImageCorrect(SystemFile file)
    {
        try
        {
            using (var image = Image.Load(file.File.FullName))
            {
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
