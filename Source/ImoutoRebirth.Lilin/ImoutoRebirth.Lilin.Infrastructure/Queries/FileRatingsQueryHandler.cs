using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class FileRatingsQueryHandler(LilinDbContext db) : IQueryHandler<FileRatingsQuery, IReadOnlyCollection<FileRating>>
{
    public async Task<IReadOnlyCollection<FileRating>> Handle(FileRatingsQuery request, CancellationToken ct)
    {
        var tags = await db.FileTags
            .Where(x => request.FileIds.Contains(x.FileId) && (x.Tag!.Name == "Favorite" || x.Tag.Name == "Rate"))
            .Select(x => new { x.FileId, x.Tag!.Name, x.Value })
            .ToListAsync(ct);

        return tags.GroupBy(x => x.FileId)
            .Select(group => new FileRating(
                group.Key,
                group.Any(x => x.Name == "Favorite"),
                int.TryParse(group.FirstOrDefault(x => x.Name == "Rate")?.Value, out var rating) ? rating : 0))
            .ToList();
    }
}
