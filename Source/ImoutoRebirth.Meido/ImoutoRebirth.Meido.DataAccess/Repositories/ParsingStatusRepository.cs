using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.Domain.ParsingStatusAggregate;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace ImoutoRebirth.Meido.DataAccess.Repositories;

internal class ParsingStatusRepository : IParsingStatusRepository
{
    private readonly MeidoDbContext _meidoDbContext;

    public ParsingStatusRepository(MeidoDbContext meidoDbContext) => _meidoDbContext = meidoDbContext;

    public async ValueTask Add(ParsingStatus parsingStatus)
        => await _meidoDbContext.ParsingStatuses.AddAsync(parsingStatus);

    public ValueTask<ParsingStatus?> Get(Guid fileId, MetadataSource source)
        => _meidoDbContext.ParsingStatuses.FindAsync(fileId, source);

    public async Task<IReadOnlyCollection<ParsingStatus>> GetBySourcePostIds(
        IReadOnlyCollection<string> postIds,
        MetadataSource source)
    {
        return await _meidoDbContext.ParsingStatuses
            .Where(x => x.Source == source && x.FileIdFromSource != null && postIds.Contains(x.FileIdFromSource))
            .ToListAsync();
    }

    public async Task<IReadOnlyCollection<ParsingStatus>> GetFaultedParsingStatuses(Instant earlierThan)
    {
        return await _meidoDbContext.ParsingStatuses
            .Where(x => StatusSet.AllFaulted.Contains(x.Status) && x.UpdatedAt < earlierThan)
            .ToListAsync();
    }
}
