using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface ITagTypeRepository
{
    Task<TagType> Get(string name, CancellationToken ct = default);
    
    Task<TagType?> Get(Guid id, CancellationToken ct = default);
    
    Task<IReadOnlyCollection<TagType>> GetAll(CancellationToken ct = default);
}
