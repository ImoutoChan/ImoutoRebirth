using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface ITagTypeRepository
    {
        Task<TagType> GetOrCreate(string name);
        Task<TagType?> Get(string name);
        Task<TagType> Create(string name);
    }
}