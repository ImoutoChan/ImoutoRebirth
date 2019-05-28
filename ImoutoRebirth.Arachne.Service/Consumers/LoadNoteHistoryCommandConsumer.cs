using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Arachne.Service.Consumers
{
    public class LoadNoteHistoryCommandConsumer : IConsumer<ILoadNoteHistoryCommand>
    {
        private readonly ILogger<LoadTagHistoryCommandConsumer> _logger;
        private readonly IArachneSearchService _arachneSearchService;

        public LoadNoteHistoryCommandConsumer(
            ILogger<LoadTagHistoryCommandConsumer> logger,
            IArachneSearchService arachneSearchService)
        {
            _logger = logger;
            _arachneSearchService = arachneSearchService;
        }

        public async Task Consume(ConsumeContext<ILoadNoteHistoryCommand> context)
        {
            _logger.LogTrace(
                "Note history requested after {LastProcessedNoteUpdate} in {SearchEngine}", 
                context.Message.LastProcessedNoteUpdateAt,
                context.Message.SearchEngineType.ToModel());

            var (changedPostIds, lastHistoryId) = await _arachneSearchService.LoadChangesForNotesSince(
                context.Message.LastProcessedNoteUpdateAt,
                context.Message.SearchEngineType.ToModel());


        }
    }
}