using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class DanbooruMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public DanbooruMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Danbooru;

    public Task SendRequestCommand(Guid fileId, string md5) 
        => _bus.Send<IDanbooruSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
}
