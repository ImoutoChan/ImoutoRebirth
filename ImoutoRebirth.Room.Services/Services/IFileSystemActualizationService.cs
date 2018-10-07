using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.Core.Services
{
    public interface IFileSystemActualizationService
    {
        Task PryColleciton(OverseedColleciton overseedColleciton);
    }
}