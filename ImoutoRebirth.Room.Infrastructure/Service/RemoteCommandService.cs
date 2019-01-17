using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Room.Core.Services.Abstract;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;

namespace ImoutoRebirth.Room.Infrastructure.Service
{
    internal class RemoteCommandService : IRemoteCommandService
    {
        private readonly IConfiguredSendEndpointProvider _configuredSendEndpointProvider;

        public RemoteCommandService(IConfiguredSendEndpointProvider configuredSendEndpointProvider)
        {
            _configuredSendEndpointProvider = configuredSendEndpointProvider;
        }
        
        public async Task UpdateMetadataRequest(Guid fileId, string md5)
        {
            var command = new
            {
                Md5 = md5,
                FileId = fileId
            };

            var requestClient 
                = await _configuredSendEndpointProvider
                   .GetSendEndpoint<IEverywhereSearchMetadataCommand>();
            await requestClient.Send<IEverywhereSearchMetadataCommand>(command);
        }
    }
}