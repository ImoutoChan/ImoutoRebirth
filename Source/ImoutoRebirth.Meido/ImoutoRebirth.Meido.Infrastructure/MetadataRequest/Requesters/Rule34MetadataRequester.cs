using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class Rule34MetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public Rule34MetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Rule34;

    public Task SendRequestCommand(Guid fileId, string md5) 
        => _bus.Send<IRule34SearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
}