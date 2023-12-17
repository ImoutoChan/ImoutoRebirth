using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public interface ITagRepository
{
    Task<Tag?> Get(string name, Guid typeId, CancellationToken ct);

    Task Update(Tag tag);

    Task Create(Tag tag);
}
