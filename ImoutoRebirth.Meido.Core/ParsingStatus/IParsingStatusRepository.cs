using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public interface IParsingStatusRepository
    {
        Task Add(ParsingStatus parsingStatus);

        Task<ParsingStatus> Get(Guid fileId, int sourceId);
    }
}