using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class GelbooruMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public GelbooruMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Gelbooru;

    public Task SendRequestCommand(Guid fileId, string md5) 
        => _bus.Send<IGelbooruSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
}
