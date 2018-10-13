using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ICollectionFileService
    {
        Task SaveNew(
            MovedInformation movedInformation,
            Guid collectionId);
    }
}