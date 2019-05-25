using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public interface IParsingService
    {
        Task CreateParsingStatusesForNewFile(Guid fileId, string md5);
    }
}