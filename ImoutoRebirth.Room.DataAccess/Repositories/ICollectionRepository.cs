using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess
{
    public interface ICollectionRepository
    {
        Task<IReadOnlyCollection<OverseedColleciton>> GetAllOverseedCollecitons();
    }
}