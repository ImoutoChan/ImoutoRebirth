using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public class ParsingService : IParsingService
    {
        private readonly IParsingStatusRepository _parsingStatusRepository;

        public ParsingService(IParsingStatusRepository parsingStatusRepository)
        {
            _parsingStatusRepository = parsingStatusRepository;
        }

        public async Task CreateParsingStatusesForNewFile(Guid fileId, string md5)
        {
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));
            ArgumentValidator.NotNullOrWhiteSpace(() => md5);

            var allMetadataSources = typeof(MetadataSource).GetEnumValues().Cast<MetadataSource>();

            foreach (var metadataSource in allMetadataSources)
            {
                var parsingStatus = ParsingStatus.Create(fileId, md5, metadataSource);
                
                await _parsingStatusRepository.Add(parsingStatus);
            }
        }

        public async Task SaveSearchResult(
            int sourceId,
            Guid fileId,
            SearchStatus resultStatus,
            int? fileIdFromSource,
            string errorText)
        {
            ArgumentValidator.IsEnumDefined(() => resultStatus);
            ArgumentValidator.Requires(() => fileId != default, nameof(fileId));

            var parsingStatus = await _parsingStatusRepository.Get(fileId, sourceId);

            switch (resultStatus)
            {
                case SearchStatus.NotFound:
                    parsingStatus.SetSearchNotFound();
                    break;
                case SearchStatus.Success:
                    ArgumentValidator.Requires(() => fileIdFromSource.HasValue, nameof(fileIdFromSource));
                    // ReSharper disable once PossibleInvalidOperationException
                    parsingStatus.SetSearchFound(fileIdFromSource.Value);
                    break;
                case SearchStatus.Error:
                    parsingStatus.SetSearchFailed(errorText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resultStatus), resultStatus, null);
            }
        }
    }
}