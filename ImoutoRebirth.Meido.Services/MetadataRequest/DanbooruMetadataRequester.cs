using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Core;
using MassTransit;

namespace ImoutoRebirth.Meido.Services.MetadataRequest
{
    public class DanbooruMetadataRequester : IMetadataRequester
    {
        private readonly IBus _bus;

        public DanbooruMetadataRequester(IBus bus)
        {
            _bus = bus;
        }

        public MetadataSource Source { get; }= MetadataSource.Danbooru;

        public async Task SendRequestCommand(Guid fileId, string md5)
        {
            await _bus.Send<IDanbooruSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
        }
    }
}