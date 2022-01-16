using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract;

public interface IFileSystemActualizationService
{
    Task PryCollection(OversawCollection oversawCollection);
}