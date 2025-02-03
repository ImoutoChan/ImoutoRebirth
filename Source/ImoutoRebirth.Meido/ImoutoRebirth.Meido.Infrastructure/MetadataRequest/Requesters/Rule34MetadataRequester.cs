using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class Rule34MetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public Rule34MetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Rule34;

    public Task SendRequestCommand(Guid fileId, string md5, string fileName)
        => _bus.Send(new Rule34SearchMetadataCommand(md5, fileId, fileName));
}
