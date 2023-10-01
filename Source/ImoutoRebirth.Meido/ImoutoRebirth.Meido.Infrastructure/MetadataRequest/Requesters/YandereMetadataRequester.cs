using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Meido.Domain;
using MassTransit;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest.Requesters;

internal class YandereMetadataRequester : IMetadataRequester
{
    private readonly IBus _bus;

    public YandereMetadataRequester(IBus bus) => _bus = bus;

    public MetadataSource Source => MetadataSource.Yandere;

    public Task SendRequestCommand(Guid fileId, string md5) 
        => _bus.Send<IYandereSearchMetadataCommand>(new {FileId = fileId, Md5 = md5});
}
