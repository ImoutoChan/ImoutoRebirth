using ImoutoRebirth.Room.Domain.CollectionAggregate;

namespace ImoutoRebirth.Room.Application.Services;

public interface ICollectionRepository
{
    Task<IReadOnlyCollection<Guid>> GetAllIds();

    Task<IReadOnlyCollection<Collection>> GetAll();

    Task<Collection?> GetById(Guid id);

    Task Create(Collection collection);
    
    Task Update(Collection collection);

    Task Remove(Guid id);
}
