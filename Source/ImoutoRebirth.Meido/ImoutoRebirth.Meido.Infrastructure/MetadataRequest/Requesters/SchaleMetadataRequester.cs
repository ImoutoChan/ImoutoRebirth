using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class SchaleMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public SchaleMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Schale;

    public Task SendRequestCommand(Guid fileId, string md5, string fileName)
        => _bus.Send(new SchaleSearchMetadataCommand(md5, fileId, fileName));
}
