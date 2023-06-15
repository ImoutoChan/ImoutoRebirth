using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

internal class FileTagRepository : IFileTagRepository
{
    private readonly LilinDbContext _lilinDbContext;

    public FileTagRepository(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<IReadOnlyCollection<FileTag>> GetForFile(Guid fileId, CancellationToken ct)
    {
        var results = await _lilinDbContext.FileTags
            .Where(x => x.FileId == fileId)
            .AsNoTracking()
            .ToArrayAsync(cancellationToken: ct);

        return results.Select(x => x.ToModel()).ToArray();
    }

    public async Task Add(FileTag fileTag)
    {
        var entity = fileTag.ToNewEntity();
        await _lilinDbContext.AddAsync(entity);
        await _lilinDbContext.SaveChangesAsync();
    }

    public async Task Delete(FileTag fileTag)
    {
        var tagsToDelete = await _lilinDbContext.FileTags.Where(
                x => x.Source == fileTag.Source
                     && x.TagId == fileTag.TagId
                     && x.FileId == fileTag.FileId
                     && x.Value == fileTag.Value)
            .ToListAsync();

        _lilinDbContext.FileTags.RemoveRange(tagsToDelete);
        await _lilinDbContext.SaveChangesAsync();
    }
}
