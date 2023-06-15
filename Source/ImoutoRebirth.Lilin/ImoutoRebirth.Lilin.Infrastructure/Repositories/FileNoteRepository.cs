using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Repositories;

internal class FileNoteRepository : IFileNoteRepository
{
    private readonly LilinDbContext _lilinDbContext;

    public FileNoteRepository(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Create(FileNote note)
    {
        await _lilinDbContext.Notes.AddAsync(note.ToNewEntity());
        await _lilinDbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<FileNote>> GetForFile(Guid fileId, CancellationToken ct)
    {
        var results = await _lilinDbContext.Notes
            .Where(x => x.FileId == fileId)
            .ToArrayAsync(cancellationToken: ct);

        return results.Select(x => x.ToModel()).ToArray();
    }

    public async Task Update(FileNote note)
    {
        var entity = await _lilinDbContext.Notes
            .Where(x => x.FileId == note.FileId && x.SourceId == note.SourceId && x.Source == note.Source)
            .FirstOrDefaultAsync();

        if (entity == null)
            throw new Exception($"Note with id {note.GetIdentity()} wasn't found");

        entity.Label = note.Label;
        entity.Height = note.Height;
        entity.Width = note.Width;
        entity.PositionFromTop = note.PositionFromTop;
        entity.PositionFromLeft = note.PositionFromLeft;

        await _lilinDbContext.SaveChangesAsync();
    }

    public async Task Delete(FileNote note)
    {
        var entity = await _lilinDbContext.Notes
            .Where(x => x.FileId == note.FileId && x.SourceId == note.SourceId && x.Source == note.Source)
            .FirstOrDefaultAsync();

        if (entity == null)
            throw new Exception($"Note with id {note.GetIdentity()} wasn't found");

        _lilinDbContext.Remove(entity);

        await _lilinDbContext.SaveChangesAsync();
    }
}
