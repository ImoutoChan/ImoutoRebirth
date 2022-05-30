using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public class ParsingService : IParsingService
    {
        private readonly IParsingStatusRepository _parsingStatusRepository;
        private readonly ILogger<ParsingService> _logger;

        public ParsingService(IParsingStatusRepository parsingStatusRepository, ILogger<ParsingService> logger)
        {
            _parsingStatusRepository = parsingStatusRepository;
            _logger = logger;
        }

        public async Task CreateParsingStatusesForNewFile(Guid fileId, string md5)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.NotNullOrWhiteSpace(() => md5);

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

                var parsingStatus = ParsingStatus.Create(fileId, md5, metadataSource);
                
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

            var parsingStatus = await _parsingStatusRepository
                .Get(fileId, (MetadataSource)sourceId);

            if (parsingStatus == null)
                throw new DomainException("ParsingStatus should not be null");
            
            switch (resultStatus)
            {
                case SearchStatus.NotFound:
                    parsingStatus.SetSearchNotFound();
                    break;
                case SearchStatus.Success:
                    ArgumentValidator.NotNull(fileIdFromSource, nameof(fileIdFromSource));
                    parsingStatus.SetSearchFound(fileIdFromSource.Value);
                    break;
                case SearchStatus.Error:
                    parsingStatus.SetSearchFailed(errorText!);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultStatus), resultStatus, null);
            }
        }

        public async Task MarkAsSaved(Guid fileId, int sourceId)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.Requires(() => sourceId >= 0 && sourceId <= 2, nameof(sourceId));

            var parsingStatus = await _parsingStatusRepository.Get(fileId, (MetadataSource)sourceId);

            if (parsingStatus == null)
                throw new DomainException("ParsingStatus should not be null");

            parsingStatus.SetSearchSaved();
        }
    }
}
