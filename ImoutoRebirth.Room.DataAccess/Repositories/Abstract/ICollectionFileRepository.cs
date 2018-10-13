using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.DataAccess.Models;

namespace ImoutoRebirth.Room.DataAccess.Repositories.Abstract
{
    public interface ICollectionFileRepository
    {
        Task Add(CollectionFile collectionFile);
        Task<bool> HasFile(Guid collectionId, string path);
    }
}