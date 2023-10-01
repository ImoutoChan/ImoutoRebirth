using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using NodaTime;

namespace ImoutoRebirth.Meido.Application.Infrastructure;

public interface IParsingStatusRepository
{
    ValueTask Add(ParsingStatus parsingStatus);

    ValueTask<ParsingStatus?> Get(Guid fileId, MetadataSource source);

    Task<IReadOnlyCollection<ParsingStatus>> GetBySourcePostIds(
        IReadOnlyCollection<int> postIds,
        MetadataSource source);

    Task<IReadOnlyCollection<ParsingStatus>> GetFaultedParsingStatuses(Instant earlierThan);
}