using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public class ParsingService : IParsingService
    {
        private readonly IParsingStatusRepository _parsingStatusRepository;
        private readonly ILogger<ParsingService> _logger;
        private readonly IClock _clock;

        public ParsingService(IParsingStatusRepository parsingStatusRepository, ILogger<ParsingService> logger, IClock clock)
        {
            _parsingStatusRepository = parsingStatusRepository;
            _logger = logger;
            _clock = clock;
        }

        public async Task CreateParsingStatusesForNewFile(Guid fileId, string md5)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.NotNullOrWhiteSpace(() => md5);

            var now = _clock.GetCurrentInstant();
            var allMetadataSources = typeof(MetadataSource).GetEnumValues().Cast<MetadataSource>();

            foreach (var metadataSource in allMetadataSources)
            {
                var check = await _parsingStatusRepository.Get(fileId, metadataSource);
                if (check != null)
                {
                    _logger.LogWarning(
                        "Can't create a parsing status with duplicate key {FileId}, {Source}",
                        fileId,
                        metadataSource);
                    continue;
                }

                var parsingStatus = ParsingStatus.Create(fileId, md5, metadataSource, now);
                
                await _parsingStatusRepository.Add(parsingStatus);
            }
        }

        public async Task SaveSearchResult(
            int sourceId,
            Guid fileId,
            SearchStatus resultStatus,
            int? fileIdFromSource,
            string? errorText)
        {
            ArgumentValidator.IsEnumDefined(() => resultStatus);
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));

            var now = _clock.GetCurrentInstant();
            var parsingStatus = await _parsingStatusRepository
                .Get(fileId, (MetadataSource)sourceId);

            if (parsingStatus == null)
                throw new DomainException("ParsingStatus should not be null");
            
            switch (resultStatus)
            {
                case SearchStatus.NotFound:
                    parsingStatus.SetSearchNotFound(now);
                    break;
                case SearchStatus.Success:
                    ArgumentValidator.NotNull(fileIdFromSource, nameof(fileIdFromSource));
                    parsingStatus.SetSearchFound(fileIdFromSource.Value, now);
                    break;
                case SearchStatus.Error:
                    parsingStatus.SetSearchFailed(errorText!, now);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultStatus), resultStatus, null);
            }
        }

        public async Task MarkAsSaved(Guid fileId, int sourceId)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.Requires(() => sourceId >= 0 && sourceId <= 2, nameof(sourceId));

            var now = _clock.GetCurrentInstant();
            var parsingStatus = await _parsingStatusRepository.Get(fileId, (MetadataSource)sourceId);

            if (parsingStatus == null)
                throw new DomainException("ParsingStatus should not be null");

            parsingStatus.SetSearchSaved(now);
        }
    }
}
