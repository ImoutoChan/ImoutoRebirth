using System;
using System.Threading.Tasks;

namespace ImoutoRebirth.Room.Core.Services.Abstract
{
    public interface ISignalSender
    {
        Task UpdateMetadataRequest(Guid fileId, string md5);
    }
}