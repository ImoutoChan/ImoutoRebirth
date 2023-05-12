using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract;

public interface ICollectionRepository
{
    Task<IReadOnlyCollection<OversawCollection>> GetAllOversaw();

    Task<IReadOnlyCollection<Collection>> GetAll();

    Task<Collection> Add(CollectionCreateData collection);

    Task Remove(Guid id);

    Task Rename(Guid id, string newName);
}