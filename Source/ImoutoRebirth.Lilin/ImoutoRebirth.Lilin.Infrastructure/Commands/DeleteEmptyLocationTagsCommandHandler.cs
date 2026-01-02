using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Commands;

internal class DeleteEmptyLocationTagsCommandHandler : ICommandHandler<DeleteEmptyLocationTagsCommand>
{
    private readonly LilinDbContext _lilinDbContext;

    public DeleteEmptyLocationTagsCommandHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Handle(DeleteEmptyLocationTagsCommand _, CancellationToken ct)
    {
        await _lilinDbContext.Tags
            .Where(x => x.Type!.Name == "Location")
            .Where(x => x.FileTags!.Count == 0)
            .ExecuteDeleteAsync(cancellationToken: ct);
    }
}
