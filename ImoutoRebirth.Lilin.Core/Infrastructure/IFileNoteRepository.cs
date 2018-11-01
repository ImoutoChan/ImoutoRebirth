using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Infrastructure
{
    public interface IFileNoteRepository : IRepository
    {
        Task Add(FileNote fileNote);
    }
}