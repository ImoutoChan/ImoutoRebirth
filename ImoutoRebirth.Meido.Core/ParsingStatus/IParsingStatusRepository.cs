using System.Threading.Tasks;

namespace ImoutoRebirth.Meido.Core.ParsingStatus
{
    public interface IParsingStatusRepository
    {
        Task Add(ParsingStatus parsingStatus);
    }
}