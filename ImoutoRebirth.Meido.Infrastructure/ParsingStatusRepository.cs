using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain.Specifications;
using ImoutoRebirth.Meido.Core;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ImoutoRebirth.Meido.Infrastructure
{
    public class ParsingStatusRepository : IParsingStatusRepository
    {
        private readonly MeidoDbContext _meidoDbContext;

        public ParsingStatusRepository(MeidoDbContext meidoDbContext)
        {
            _meidoDbContext = meidoDbContext;
        }

        public async ValueTask Add(ParsingStatus parsingStatus)
            => await _meidoDbContext.ParsingStatuses
                                    .AddAsync(parsingStatus);

        public ValueTask<ParsingStatus?> Get(Guid fileId, MetadataSource source)
            => _meidoDbContext.ParsingStatuses
                              .FindAsync(fileId, source);

        public async Task<IReadOnlyCollection<ParsingStatus>> Find(Specification<ParsingStatus> specification)
            => await _meidoDbContext.ParsingStatuses
                                    .Where(specification.ToExpression())
                                    .ToArrayAsync();
    }
}
