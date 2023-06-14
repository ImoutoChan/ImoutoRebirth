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

    public async Task Handle(UpdateTagsCountersCommand request, CancellationToken cancellationToken)
    {
        const string script = 
            $"""
             UPDATE "{nameof(LilinDbContext.Tags)}" tags
             SET "{nameof(TagEntity.Count)}" = usages.count
             FROM
             (
                SELECT id, count(*) AS count
                FROM (
                    SELECT "{nameof(FileTagEntity.TagId)}" AS id, "{nameof(FileTagEntity.FileId)}" AS fileId
                    FROM "{nameof(LilinDbContext.FileTags)}"
                    GROUP BY id, fileId) as inn
                GROUP BY id
             ) usages
             WHERE tags."{nameof(TagEntity.Id)}" = usages.id
             """;

        await _lilinDbContext.Database.ExecuteSqlRawAsync(script, cancellationToken: cancellationToken);
    }
}