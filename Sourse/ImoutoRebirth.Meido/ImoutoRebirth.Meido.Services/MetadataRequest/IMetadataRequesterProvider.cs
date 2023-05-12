using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.Services.MetadataRequest;

public interface IMetadataRequesterProvider
{
    IMetadataRequester Get(MetadataSource source);
}