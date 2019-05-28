using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Service.Consumers
{
    public class LoadTagHistoryCommandConsumer : IConsumer<ILoadTagHistoryCommand>
    {
        private readonly ILogger<LoadTagHistoryCommandConsumer> _logger;
        private readonly IArachneSearchService _arachneSearchService;

        public LoadTagHistoryCommandConsumer(
            ILogger<LoadTagHistoryCommandConsumer> logger,
            IArachneSearchService arachneSearchService)
        {
            _logger = logger;
            _arachneSearchService = arachneSearchService;
        }

        public async Task Consume(ConsumeContext<ILoadTagHistoryCommand> context)
        {
            _logger.LogTrace(
                "Tag history requested from history id {HistoryId} in {SearchEngine}", 
                context.Message.LastProcessedTagHistoryId,
                context.Message.SearchEngineType);

            var (changedPostIds, lastHistoryId) = await _arachneSearchService.LoadChangesForTagsSinceHistoryId(
                context.Message.LastProcessedTagHistoryId,
                context.Message.SearchEngineType);


        }
    }
}