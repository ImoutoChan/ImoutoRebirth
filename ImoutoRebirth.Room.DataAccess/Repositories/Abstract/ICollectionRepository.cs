using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface ICollectionRepository
    {
        Task<IReadOnlyCollection<OversawCollection>> GetAllOversawCollections();
    }
}