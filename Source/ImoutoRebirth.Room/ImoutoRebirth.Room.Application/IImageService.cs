using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application;

public interface IImageService
{
    bool IsImageCorrect(SystemFile fileInfo);
}
