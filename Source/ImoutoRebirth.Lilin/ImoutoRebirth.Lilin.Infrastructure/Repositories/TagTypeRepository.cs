using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Core.TagTypeAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

internal class TagTypeRepository : ITagTypeRepository
{
    private readonly LilinDbContext _lilinDbContext;

    public TagTypeRepository(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<TagType> Get(string name, CancellationToken ct)
    {
        var type = await _lilinDbContext.TagTypes.FirstOrDefaultAsync(x => x.Name == name, ct);

        if (type != null) 
            return type.ToModel();
        
        var newType = new TagTypeEntity
        {
            Id = Guid.NewGuid(),
            Name = name
        };

        await _lilinDbContext.TagTypes.AddAsync(newType, ct);
        await _lilinDbContext.SaveChangesAsync(ct);

        return newType.ToModel();
    }

    public async Task<TagType?> Get(Guid id, CancellationToken ct) 
        => (await _lilinDbContext.TagTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken: ct))?.ToModel();

    public async Task<IReadOnlyCollection<TagType>> GetAll(CancellationToken ct) 
        => await _lilinDbContext.TagTypes.Select(x => x.ToModel()).ToArrayAsync(cancellationToken: ct);
}
