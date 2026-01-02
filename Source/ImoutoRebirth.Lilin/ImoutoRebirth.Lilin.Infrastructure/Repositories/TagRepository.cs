using ImoutoRebirth.Common;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

internal class TagRepository : ITagRepository
{
    private readonly LilinDbContext _lilinDbContext;

    public TagRepository(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<Tag?> Get(TagIdentifier tagIdentifier, CancellationToken ct)
    {
        var result = await _lilinDbContext.Tags
            .Include(x => x.Type)
            .SingleOrDefaultAsync(
                x => x.Name == tagIdentifier.Name && x.TypeId == tagIdentifier.TypeId,
                cancellationToken: ct);

        return result?.ToModel();
    }

    public async Task<IReadOnlyCollection<Tag>> GetBatch(
        IReadOnlyCollection<TagIdentifier> tags,
        CancellationToken ct)
    {
        if (!tags.Any())
            return [];

        return await _lilinDbContext.Tags
            .Where(x => _lilinDbContext.AsQueryable(tags).Any(t => x.Name == t.Name && x.TypeId == t.TypeId))
            .Include(x => x.Type)
            .Select(x => x.ToModel())
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<Tag>> GetBatch(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.None())
            return [];

        return await _lilinDbContext.Tags
            .Where(x => ids.Contains(x.Id))
            .Include(x => x.Type)
            .Select(x => x.ToModel())
            .ToListAsync(ct);
    }

    public async Task Update(Tag tag)
    {
        var loadedTag = await _lilinDbContext.Tags.FirstAsync(x => x.Id == tag.Id);
        
        loadedTag.HasValue = tag.HasValue;
        loadedTag.SynonymsArray = tag.Synonyms;
        loadedTag.Options = tag.Options;

        await _lilinDbContext.SaveChangesAsync();
    }

    public async Task Create(Tag tag)
    {
        var newEntity = new TagEntity
        {
            Id = tag.Id,
            Name = tag.Name,
            HasValue = tag.HasValue,
            SynonymsArray = tag.Synonyms,
            TypeId = tag.Type.Id,
            Options = tag.Options
        };

        await _lilinDbContext.AddAsync(newEntity);
        await _lilinDbContext.SaveChangesAsync();
    }

    public async Task CreateBatch(IReadOnlyCollection<Tag> tags)
    {
        var entities = tags.Select(x => new TagEntity
        {
            Id = x.Id,
            Name = x.Name,
            HasValue = x.HasValue,
            SynonymsArray = x.Synonyms,
            TypeId = x.Type.Id,
            Options = x.Options
        }).ToList();

        await _lilinDbContext.Tags.AddRangeAsync(entities);
        await _lilinDbContext.SaveChangesAsync();
    }
}
