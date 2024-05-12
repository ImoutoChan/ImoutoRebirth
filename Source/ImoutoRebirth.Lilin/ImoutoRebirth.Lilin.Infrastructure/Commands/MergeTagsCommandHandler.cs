using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Commands;

internal class MergeTagsCommandHandler : ICommandHandler<MergeTagsCommand>
{
    private readonly LilinDbContext _lilinDbContext;

    public MergeTagsCommandHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Handle(MergeTagsCommand command, CancellationToken ct)
    {
        var (tagToCleanId, tagToEnrichId) = command;
        
        var tagToClean = await _lilinDbContext.Tags
            .FirstOrDefaultAsync(x => x.Id == tagToCleanId, cancellationToken: ct);
        
        if (tagToClean == null)
            throw new InvalidOperationException($"Tag with id {tagToCleanId} wasn't found");

        var fileTagsToModify = await _lilinDbContext.FileTags
            .Where(x => x.TagId == tagToCleanId)
            .ToListAsync(cancellationToken: ct);

        foreach (var fileTag in fileTagsToModify) 
            fileTag.TagId = tagToEnrichId;
        
        await _lilinDbContext.SaveChangesAsync(ct);
    }
}
