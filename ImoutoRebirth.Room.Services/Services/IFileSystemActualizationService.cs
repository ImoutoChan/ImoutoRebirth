using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public interface IFileSystemActualizationService
    {
        Task PryCollection(OversawCollection oversawCollection);
    }
}