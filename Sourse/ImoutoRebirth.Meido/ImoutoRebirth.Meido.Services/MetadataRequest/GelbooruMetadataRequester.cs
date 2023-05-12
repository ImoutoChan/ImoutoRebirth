using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Core;
using MassTransit;

namespace ImoutoRebirth.Meido.Services.MetadataRequest;

public class GelbooruMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public GelbooruMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Gelbooru;

    public async Task SendRequestCommand(Guid fileId, string md5) 
        => await _bus.Send<IGelbooruSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
}
