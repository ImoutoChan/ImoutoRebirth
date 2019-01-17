using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface IRemoteCommandService
    {
        Task UpdateMetadataRequest(Guid fileId, string md5);
    }
}