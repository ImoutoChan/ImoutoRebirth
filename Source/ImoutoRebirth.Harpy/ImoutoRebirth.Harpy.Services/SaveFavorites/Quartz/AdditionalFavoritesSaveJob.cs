using ImoutoRebirth.Harpy.Services.SaveFavorites.Commands;
using MediatR;
using Quartz;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Quartz;

[DisallowConcurrentExecution]
internal class AdditionalFavoritesSaveJob : IJob
{
    private readonly IMediator _mediator;

    public AdditionalFavoritesSaveJob(IMediator mediator) => _mediator = mediator;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken ct)
    {
        if (!await FavoritesSaveJob.Lock.WaitAsync(0, ct))
            return;

        try
        {
            await _mediator.Send(new FavoritesSaveCommand(), ct);
        }
        finally
        {
            FavoritesSaveJob.Lock.Release();
        }
    }
}
