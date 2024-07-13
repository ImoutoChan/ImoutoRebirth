using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.Persistence;

public record TagIdentifier(string Name, Guid TypeId);

public interface ITagRepository
{
    Task<Tag?> Get(TagIdentifier tagIdentifier, CancellationToken ct = default);

    Task<IReadOnlyCollection<Tag>> GetBatch(
        IReadOnlyCollection<TagIdentifier> tags,
        CancellationToken ct = default);

    Task Update(Tag tag);

    Task Create(Tag tag);

    Task CreateBatch(IReadOnlyCollection<Tag> tags);
}
