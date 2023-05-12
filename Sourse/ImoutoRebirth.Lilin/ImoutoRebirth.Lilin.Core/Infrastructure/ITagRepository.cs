using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure;

public interface ITagRepository
{
    Task<Tag?> Get(string name, Guid typeId, CancellationToken ct);

    Task<Tag?> Get(Guid id, CancellationToken ct);

    Task<IReadOnlyCollection<Tag>> Search(string? requestSearchPattern, int requestLimit, CancellationToken ct);
        
    Task Update(Tag tag);

    Task Create(Tag tag);

    Task UpdateTagsCounters();
}
