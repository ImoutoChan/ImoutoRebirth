using System.Collections;
using ImoutoRebirth.Room.Domain;

namespace ImoutoRebirth.Room.Application;

public interface ICollectionRepository
{
    Task<IReadOnlyCollection<Guid>> GetAllIds();

    Task<IReadOnlyCollection<Collection>> GetAll();

    Task<Collection?> GetById(Guid id);

    Task<Collection> Create(string name);

    Task Remove(Guid id);

    Task Rename(Guid id, string newName);
}
