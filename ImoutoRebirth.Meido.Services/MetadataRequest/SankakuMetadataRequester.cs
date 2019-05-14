using System;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Core;
using MassTransit;

namespace ImoutoRebirth.Meido.Services.MetadataRequest
{
    public class SankakuMetadataRequester : IMetadataRequester
    {
        private readonly IBus _bus;

        public SankakuMetadataRequester(IBus bus)
        {
            _bus = bus;
        }

        public MetadataSource Source { get; }= MetadataSource.Sankaku;

        public async Task SendRequestCommand(Guid fileId, string md5)
        {
            await _bus.Send<ISankakuSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
        }
    }
}