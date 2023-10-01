using ImoutoRebirth.Room.Core.Services.Abstract;

namespace ImoutoRebirth.Room.Infrastructure.Service;

public class ImageService : IImageService
{
    public bool IsImageCorrect(FileInfo fileInfo)
    {
        try
        {
            using (var image = Image.Load(fileInfo.FullName))
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