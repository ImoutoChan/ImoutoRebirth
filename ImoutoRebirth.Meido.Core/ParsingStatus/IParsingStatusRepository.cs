using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain.Specifications;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public interface IParsingStatusRepository
    {
        ValueTask Add(ParsingStatus parsingStatus);

        ValueTask<ParsingStatus?> Get(Guid fileId, MetadataSource source);

        Task<IReadOnlyCollection<ParsingStatus>> Find(Specification<ParsingStatus> specification);
    }
}
