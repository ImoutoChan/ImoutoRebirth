using System;
using System.Threading.Tasks;
using ImoutoRebirth.Room.Core.Models;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface IRemoteCommandService
    {
        Task UpdateMetadataRequest(Guid fileId, string md5);

        Task SaveTags(Guid fileId, MovedInformation movedInformation);
    }
}