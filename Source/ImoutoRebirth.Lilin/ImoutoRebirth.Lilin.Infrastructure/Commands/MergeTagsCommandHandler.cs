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
        if (tagToCleanId == tagToEnrichId)
            throw new ArgumentException("A tag cannot be merged into itself.", nameof(command));
        
        var tagToClean = await _lilinDbContext.Tags
            .FirstOrDefaultAsync(x => x.Id == tagToCleanId, cancellationToken: ct);
        
        if (tagToClean == null)
            throw new InvalidOperationException($"Tag with id {tagToCleanId} wasn't found");

        var affectedFileIds = _lilinDbContext.FileTags
            .Where(x => x.TagId == tagToCleanId)
            .Select(x => x.FileId);

        var bindings = await _lilinDbContext.FileTags
            .Where(x => (x.TagId == tagToCleanId || x.TagId == tagToEnrichId) && affectedFileIds.Contains(x.FileId))
            .OrderByDescending(x => x.TagId == tagToEnrichId)
            .ThenBy(x => x.AddedOn)
            .ToListAsync(ct);

        foreach (var group in bindings.GroupBy(x => (x.FileId, x.Source, x.Value)))
        {
            group.First().TagId = tagToEnrichId;
            _lilinDbContext.FileTags.RemoveRange(group.Skip(1));
        }
        
        await _lilinDbContext.SaveChangesAsync(ct);
    }
}
