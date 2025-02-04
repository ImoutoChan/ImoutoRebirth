using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class YandereMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public YandereMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Yandere;

    public Task SendRequestCommand(Guid fileId, string md5, string fileName)
        => _bus.Send(new YandereSearchMetadataCommand(md5, fileId, fileName));
}
