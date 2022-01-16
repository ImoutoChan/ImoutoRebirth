using ImoutoRebirth.Room.Core.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract;

public interface ICollectionFileService
{
    Task<Guid> SaveNew(
        MovedInformation movedInformation,
        Guid collectionId);

    Task Delete(Guid id);
}