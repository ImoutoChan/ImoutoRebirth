using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core.ParsingStatus;

namespace ImoutoRebirth.Meido.Core.FaultTolerance
{
    internal class FaultToleranceService : IFaultToleranceService
    {
        private readonly IParsingStatusRepository _parsingStatusRepository;

        public FaultToleranceService(IParsingStatusRepository parsingStatusRepository)
        {
            _parsingStatusRepository = parsingStatusRepository;
        }

        public async Task RequeueFaults()
        {
            var faultedParsingStatuses = await _parsingStatusRepository.Find(new AllFaultedStatusesSpecification());

            foreach (var parsingStatus in faultedParsingStatuses)
            {
                parsingStatus.RequestMetadataUpdate();
            }
        }
    }
}