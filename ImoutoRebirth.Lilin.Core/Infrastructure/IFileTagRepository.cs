using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileTagRepository : IRepository
    {
        Task Add(FileTag fileTag);
    }
}