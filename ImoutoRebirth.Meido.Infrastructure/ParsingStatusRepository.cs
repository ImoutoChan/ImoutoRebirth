using System;
using System.Threading.Tasks;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.DataAccess;

namespace ImoutoRebirth.Meido.Infrastructure
{
    public class ParsingStatusRepository : IParsingStatusRepository
    {
        private readonly MeidoDbContext _meidoDbContext;

        public ParsingStatusRepository(MeidoDbContext meidoDbContext)
        {
            _meidoDbContext = meidoDbContext;
        }

        public Task Add(ParsingStatus parsingStatus)
            => _meidoDbContext.ParsingStatuses.AddAsync(parsingStatus);

        public Task<ParsingStatus> Get(Guid fileId, int sourceId)
            => _meidoDbContext.ParsingStatuses.FindAsync(fileId, (MetadataSource)sourceId);
    }
}
