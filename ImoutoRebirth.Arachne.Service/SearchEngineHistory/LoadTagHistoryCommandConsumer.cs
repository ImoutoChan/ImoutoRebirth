using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Service.SearchEngineHistory
{

    public class LoadTagHistoryCommandConsumer : IConsumer<ILoadTagHistoryCommand>
    {
        private readonly ILogger<LoadTagHistoryCommandConsumer> _logger;
        private readonly IArachneSearchService _arachneSearchService;
        private readonly SearchEngineHistoryAccessor _searchEngineHistoryAccessor;

        public LoadTagHistoryCommandConsumer(
            ILogger<LoadTagHistoryCommandConsumer> logger,
            IArachneSearchService arachneSearchService,
            TagsSearchEngineHistoryAccessor searchEngineHistoryAccessor)
        {
            _logger = logger;
            _arachneSearchService = arachneSearchService;
            _searchEngineHistoryAccessor = searchEngineHistoryAccessor;
        }

        public async Task Consume(ConsumeContext<ILoadTagHistoryCommand> context)
        {
            var lastProcessedTagHistoryId = context.Message.LastProcessedTagHistoryId;
            var searchEngineType = context.Message.SearchEngineType.ToModel();

            if (!_searchEngineHistoryAccessor.GainAccess(searchEngineType))
            {
                _logger.LogWarning(
                    "Tag history requested from history id {HistoryId} in {SearchEngine}, " 
                    + "but last request hasn't been completed yet.",
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

            // todo send signals
        }
    }
}