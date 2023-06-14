using ImoutoRebirth.Lilin.Core.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface ITagRepository
{
    Task<Tag?> Get(string name, Guid typeId, CancellationToken ct);

    Task<Tag?> Get(Guid id, CancellationToken ct);

    Task Update(Tag tag);

    Task Create(Tag tag);
}
