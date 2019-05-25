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
    }
}