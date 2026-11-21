using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Queries;

internal class UpdateTagsCountersCommandHandler : ICommandHandler<UpdateTagsCountersCommand>
{
    private readonly LilinDbContext _lilinDbContext;

    public UpdateTagsCountersCommandHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Handle(UpdateTagsCountersCommand request, CancellationToken ct)
    {
        const string script = 
            $"""
             UPDATE "{nameof(LilinDbContext.Tags)}" tags
             SET "{nameof(TagEntity.Count)}" = usages.count
             FROM (
                 SELECT t."{nameof(TagEntity.Id)}" AS id, count(DISTINCT f."{nameof(FileTagEntity.FileId)}") AS count
                 FROM "{nameof(LilinDbContext.Tags)}" t
                 LEFT JOIN "{nameof(LilinDbContext.FileTags)}" f ON f."{nameof(FileTagEntity.TagId)}" = t."{nameof(TagEntity.Id)}"
                 GROUP BY t."{nameof(TagEntity.Id)}"
             ) usages
             WHERE tags."{nameof(TagEntity.Id)}" = usages.id
               AND tags."{nameof(TagEntity.Count)}" <> usages.count
             """;

        await _lilinDbContext.Database.ExecuteSqlRawAsync(script, cancellationToken: ct);
    }
}