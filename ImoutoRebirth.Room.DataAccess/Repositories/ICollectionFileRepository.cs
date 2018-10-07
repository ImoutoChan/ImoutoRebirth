using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess
{
    public interface ICollectionFileRepository
    {
        Task Add(CollectionFile collectionFile);
    }
}