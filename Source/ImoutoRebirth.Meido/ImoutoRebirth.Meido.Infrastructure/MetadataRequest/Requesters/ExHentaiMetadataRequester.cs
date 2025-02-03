using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class ExHentaiMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public ExHentaiMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.ExHentai;

    public Task SendRequestCommand(Guid fileId, string md5, string fileName)
        => _bus.Send(new ExHentaiSearchMetadataCommand(md5, fileId, fileName));
}
