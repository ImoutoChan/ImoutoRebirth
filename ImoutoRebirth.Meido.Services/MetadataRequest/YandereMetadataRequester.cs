using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Core;
using MassTransit;

namespace ImoutoRebirth.Meido.Services.MetadataRequest
{
    public class YandereMetadataRequester : IMetadataRequester
    {
        private readonly IBus _bus;

        public YandereMetadataRequester(IBus bus)
        {
            _bus = bus;
        }

        public MetadataSource Source => MetadataSource.Yandere;

        public async Task SendRequestCommand(Guid fileId, string md5)
        {
            await _bus.Send<IYandereSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
        }
    }
}
