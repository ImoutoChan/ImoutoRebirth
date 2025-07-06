using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory;

public class LoadTagHistoryCommandConsumer : IConsumer<LoadTagHistoryCommand>
{
    private readonly ILogger<LoadTagHistoryCommandConsumer> _logger;
    private readonly IArachneSearchService _arachneSearchService;
    private readonly SearchEngineHistoryAccessor _searchEngineHistoryAccessor;
    private readonly IBus _bus;

    public LoadTagHistoryCommandConsumer(
        ILogger<LoadTagHistoryCommandConsumer> logger,
        IArachneSearchService arachneSearchService,
        TagsSearchEngineHistoryAccessor searchEngineHistoryAccessor,
        IBus bus)
    {
        _logger = logger;
        _arachneSearchService = arachneSearchService;
        _searchEngineHistoryAccessor = searchEngineHistoryAccessor;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<LoadTagHistoryCommand> context)
    {
        var expired = (DateTime.UtcNow - context.SentTime) > TimeSpan.FromMinutes(1);
        if (expired)
        {
            _logger.LogWarning("Skip tag history request from {SearchEngine} because it's expired",
                context.Message.SearchEngineType);
            return;
        }
        
        var lastProcessedTagHistoryId = context.Message.LastProcessedTagHistoryId;
        var searchEngineType = context.Message.SearchEngineType.ToModel();

        if (!_searchEngineHistoryAccessor.GainAccess(searchEngineType))
        {
            _logger.LogWarning(
                "Tag history requested from history id {HistoryId} in {SearchEngine}, " 
                + "but last request hasn't been completed yet",
                lastProcessedTagHistoryId,
                searchEngineType);
            return;
        }
        try
        {
            await ConsumeInternal(lastProcessedTagHistoryId, searchEngineType);
        }
        finally
        {
            _searchEngineHistoryAccessor.ReleaseAccess(searchEngineType);
        }
    }

    private async Task ConsumeInternal(int lastProcessedTagHistoryId, SearchEngineType searchEngineType)
    {
        _logger.LogInformation(
            "Tag history requested from history id {HistoryId} in {SearchEngine}",
            lastProcessedTagHistoryId,
            searchEngineType);

        var (changedPostIds, lastHistoryId) = await _arachneSearchService.LoadChangesForTagsSinceHistoryId(
            lastProcessedTagHistoryId,
            searchEngineType);

        await _bus.Send(new TagsUpdatedCommand((int)searchEngineType, changedPostIds.ToArray(), lastHistoryId));
    }
}
