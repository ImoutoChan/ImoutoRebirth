using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class SankakuMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public SankakuMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Sankaku;

    public Task SendRequestCommand(Guid fileId, string md5, string fileName)
        => _bus.Send(new SankakuSearchMetadataCommand(md5, fileId, fileName));
}
