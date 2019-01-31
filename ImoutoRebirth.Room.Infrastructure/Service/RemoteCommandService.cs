using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Room.Core.Services.Abstract;
using MassTransit;

namespace ImoutoRebirth.Room.Infrastructure.Service
{
    internal class RemoteCommandService : IRemoteCommandService
    {
        private readonly IBus _bus;

        public RemoteCommandService(IBus bus)
        {
            _bus = bus;
        }
        
        public async Task UpdateMetadataRequest(Guid fileId, string md5)
        {
            var command = new
            {
                Md5 = md5,
                FileId = fileId
            };

            await _bus.Send<IEverywhereSearchMetadataCommand>(command);
        }
    }
}