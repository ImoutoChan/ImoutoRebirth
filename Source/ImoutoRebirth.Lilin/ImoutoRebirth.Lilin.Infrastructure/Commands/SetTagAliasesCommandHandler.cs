using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Commands;

internal class SetTagAliasesCommandHandler : ICommandHandler<SetTagAliasesCommand>
{
    private readonly LilinDbContext _lilinDbContext;

    public SetTagAliasesCommandHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Handle(SetTagAliasesCommand command, CancellationToken ct)
    {
        var (tagId, aliasTagIds) = command;

        await _lilinDbContext.TagAliases
            .Where(x => x.TagId == tagId || x.AliasTagId == tagId)
            .ExecuteDeleteAsync(cancellationToken: ct);

        var newPairs = aliasTagIds
            .Where(x => x != tagId)
            .Distinct()
            .Select(x => new TagAliasEntity
            {
                TagId = tagId,
                AliasTagId = x
            });

        _lilinDbContext.TagAliases.AddRange(newPairs);
        await _lilinDbContext.SaveChangesAsync(ct);
    }
}
