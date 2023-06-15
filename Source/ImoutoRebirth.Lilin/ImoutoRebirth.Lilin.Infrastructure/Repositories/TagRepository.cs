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

    public async Task<Tag?> Get(string name, Guid typeId, CancellationToken ct)
    {
        var result = await _lilinDbContext.Tags
            .Include(x => x.Type)
            .SingleOrDefaultAsync(x => x.Name == name && x.TypeId == typeId, cancellationToken: ct);

        return result?.ToModel();
    }

    public async Task<Tag?> Get(Guid id, CancellationToken ct)
    {
        var result = await _lilinDbContext.Tags
            .Include(x => x.Type)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

        return result?.ToModel();
    }

    public async Task Update(Tag tag)
    {
        var loadedTag = await _lilinDbContext.Tags.FirstAsync(x => x.Id == tag.Id);
        
        loadedTag.HasValue = tag.HasValue;
        loadedTag.SynonymsArray = tag.Synonyms;

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
            TypeId = tag.Type.Id
        };

        await _lilinDbContext.AddAsync(newEntity);
        await _lilinDbContext.SaveChangesAsync();
    }
}
