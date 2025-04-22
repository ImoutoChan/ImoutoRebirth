using ImoutoRebirth.Meido.Domain;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest;

internal interface IMetadataRequester
{
    MetadataSource Source { get; }

    Task SendRequestCommand(Guid fileId, string md5, string fileName);
}
