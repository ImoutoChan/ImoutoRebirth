using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure;

public interface ITagTypeRepository
{
    Task<TagType?> Get(string name);
    Task<TagType?> Get(Guid id);
    Task<TagType> Create(string name);
    Task<IReadOnlyCollection<TagType>> GetAll();
        
}