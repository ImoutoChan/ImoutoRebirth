using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Core.FaultTolerance;

internal class FaultToleranceService : IFaultToleranceService
{
    private readonly IParsingStatusRepository _parsingStatusRepository;
    private readonly ILogger<FaultToleranceService> _logger;
    private readonly IClock _clock;

    public FaultToleranceService(
        IParsingStatusRepository parsingStatusRepository, 
        ILogger<FaultToleranceService> logger,
        IClock clock)
    {
        _parsingStatusRepository = parsingStatusRepository;
        _logger = logger;
        _clock = clock;
    }

    public async Task RequeueFaults()
    {
        var faultedParsingStatuses = await _parsingStatusRepository.Find(new AllFaultedStatusesSpecification());

        _logger.LogInformation("Requesting update after faulted for {Count} posts", faultedParsingStatuses.Count);

        foreach (var parsingStatus in faultedParsingStatuses)
        {
            parsingStatus.RequestMetadataUpdate(_clock.GetCurrentInstant());
        }
    }
}
