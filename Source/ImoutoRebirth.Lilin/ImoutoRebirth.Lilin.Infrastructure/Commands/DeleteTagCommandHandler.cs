using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.TagSlice;
using ImoutoRebirth.Lilin.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Lilin.Infrastructure.Commands;

internal class DeleteTagCommandHandler : ICommandHandler<DeleteTagCommand>
{
    private readonly LilinDbContext _lilinDbContext;

    public DeleteTagCommandHandler(LilinDbContext lilinDbContext) => _lilinDbContext = lilinDbContext;

    public async Task Handle(DeleteTagCommand command, CancellationToken ct)
    {
        var tagToDeleteId = command.TagToDeleteId;

        await _lilinDbContext.Tags
            .Where(x => x.Id == tagToDeleteId)
            .ExecuteDeleteAsync(cancellationToken: ct);
    }
}
