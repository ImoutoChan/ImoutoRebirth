using ImoutoRebirth.Meido.Domain;

namespace ImoutoRebirth.Meido.Application.Infrastructure;

public interface ISourceMetadataRequester
{
    Task Request(MetadataSource source, Guid fileId, string md5);
}
