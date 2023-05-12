using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.Services.MetadataRequest;

public interface IMetadataRequester
{
    MetadataSource Source { get; }

    Task SendRequestCommand(Guid fileId, string md5);
}