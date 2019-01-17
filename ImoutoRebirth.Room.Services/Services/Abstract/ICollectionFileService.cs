using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ICollectionFileService
    {
        Task<Guid> SaveNew(
            MovedInformation movedInformation,
            Guid collectionId);
    }
}