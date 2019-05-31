using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory
{
    public class LoadNoteHistoryCommandConsumer : IConsumer<ILoadNoteHistoryCommand>
    {
        private readonly ILogger<LoadTagHistoryCommandConsumer> _logger;
        private readonly IArachneSearchService _arachneSearchService;
        private readonly SearchEngineHistoryAccessor _searchEngineHistoryAccessor;

        public LoadNoteHistoryCommandConsumer(
            ILogger<LoadTagHistoryCommandConsumer> logger,
            IArachneSearchService arachneSearchService,
            NotesSearchEngineHistoryAccessor searchEngineHistoryAccessor)
        {
            _logger = logger;
            _arachneSearchService = arachneSearchService;
            _searchEngineHistoryAccessor = searchEngineHistoryAccessor;
        }

        public async Task Consume(ConsumeContext<ILoadNoteHistoryCommand> context)
        {
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

            var (changedPostIds, lastHistoryId) = await _arachneSearchService.LoadChangesForNotesSince(
                                                      lastProcessedNoteUpdateAt,
                                                      searchEngineType);

            // todo send results
        }
    }
}