using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using SearchEngineType = ImoutoRebirth.Arachne.Core.Models.SearchEngineType;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory;

public class LoadNoteHistoryCommandConsumer : IConsumer<LoadNoteHistoryCommand>
{
    private readonly ILogger<LoadNoteHistoryCommandConsumer> _logger;
    private readonly IArachneSearchService _arachneSearchService;
    private readonly SearchEngineHistoryAccessor _searchEngineHistoryAccessor;
    private readonly IBus _bus;

    public LoadNoteHistoryCommandConsumer(
        ILogger<LoadNoteHistoryCommandConsumer> logger,
        IArachneSearchService arachneSearchService,
        NotesSearchEngineHistoryAccessor searchEngineHistoryAccessor,
        IBus bus)
    {
        _logger = logger;
        _arachneSearchService = arachneSearchService;
        _searchEngineHistoryAccessor = searchEngineHistoryAccessor;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<LoadNoteHistoryCommand> context)
    {
        var expired = (DateTime.UtcNow - context.SentTime) > TimeSpan.FromMinutes(1);
        if (expired)
        {
            _logger.LogWarning("Skip note history request from {SearchEngine} because it's expired",
                context.Message.SearchEngineType);
            return;
        }
        
        var lastProcessedNoteUpdateAt = context.Message.LastProcessedNoteUpdateAt;
        var searchEngineType = context.Message.SearchEngineType.ToModel();

        if (!_searchEngineHistoryAccessor.GainAccess(searchEngineType))
        {
            _logger.LogWarning(
                "Note history requested after {LastProcessedNoteUpdate} in {SearchEngine}, "
                + "but last request hasn't been completed yet.",
                lastProcessedNoteUpdateAt,
                searchEngineType);
            return;
        }
        try
        {
            await ConsumeInternal(lastProcessedNoteUpdateAt, searchEngineType);
        }
        finally
        {
            _searchEngineHistoryAccessor.ReleaseAccess(searchEngineType);
        }
    }

    private async Task ConsumeInternal(DateTimeOffset lastProcessedNoteUpdateAt, SearchEngineType searchEngineType)
    {
        _logger.LogTrace(
            "Note history requested after {LastProcessedNoteUpdate} in {SearchEngine}",
            lastProcessedNoteUpdateAt,
            searchEngineType);

        var (changedPostIds, lastNoteUpdateDate) = await _arachneSearchService.LoadChangesForNotesSince(
            lastProcessedNoteUpdateAt,
            searchEngineType);

        await _bus.Send(new NotesUpdatedCommand((int)searchEngineType, changedPostIds, lastNoteUpdateDate));
    }
}
