using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Core.FaultTolerance
{
    internal class FaultToleranceService : IFaultToleranceService
    {
        private readonly IParsingStatusRepository _parsingStatusRepository;
        private readonly ILogger<FaultToleranceService> _logger;

        public FaultToleranceService(
            IParsingStatusRepository parsingStatusRepository, 
            ILogger<FaultToleranceService> logger)
        {
            _parsingStatusRepository = parsingStatusRepository;
            _logger = logger;
        }

        public async Task RequeueFaults()
        {
            var faultedParsingStatuses = await _parsingStatusRepository.Find(new AllFaultedStatusesSpecification());

            _logger.LogInformation("Requesting update after faulted for {Count} posts", faultedParsingStatuses.Count);

            foreach (var parsingStatus in faultedParsingStatuses)
            {
                parsingStatus.RequestMetadataUpdate();
            }
        }
    }
}