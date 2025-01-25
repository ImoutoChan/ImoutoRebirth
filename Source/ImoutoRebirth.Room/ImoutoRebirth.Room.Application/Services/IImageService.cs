using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application.Services;

public interface IImageService
{
    bool IsImageCorrect(SystemFile fileInfo);

    bool IsImageHasSupportedExtension(SystemFile file);
}
