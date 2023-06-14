using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Mappers;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class FileInfoQueryHandler : IQueryHandler<FileInfoQuery, DetailedFileInfo>
{
    private readonly LilinDbContext _lilinDbContext;

    public FileInfoQueryHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task<DetailedFileInfo> Handle(FileInfoQuery request, CancellationToken ct)
    {
        var fileTags = await _lilinDbContext.FileTags
            .Include(x => x.Tag)
            .ThenInclude(x => x!.Type)
            .Where(x => x.FileId == request.FileId)
            .ToListAsync(cancellationToken: ct);
        
        var fileNotes = await _lilinDbContext.Notes
            .Where(x => x.FileId == request.FileId)
            .ToListAsync(cancellationToken: ct);

        var tags = fileTags
            .Select(x => new DetailedFileTag(request.FileId, x.Tag!.ToModel(), x.Value, x.Source))
            .ToList();

        var notes = fileNotes
            .Select(x => x.ToModel())
            .ToList();

        return new DetailedFileInfo(request.FileId, tags, notes);
    }
}