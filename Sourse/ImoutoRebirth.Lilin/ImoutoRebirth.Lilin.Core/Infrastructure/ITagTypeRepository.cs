using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure;

public interface ITagTypeRepository
{
    Task<TagType?> Get(string name, CancellationToken ct);
    
    Task<TagType?> Get(Guid id, CancellationToken ct);
    
    Task<IReadOnlyCollection<TagType>> GetAll(CancellationToken ct);
    
    Task<TagType> Create(string name);
        
}
