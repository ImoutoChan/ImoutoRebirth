using ImoutoRebirth.Meido.Application.Infrastructure;
using ImoutoRebirth.Meido.Domain;

namespace ImoutoRebirth.Meido.Infrastructure.MetadataRequest;

internal class SourceMetadataRequester : ISourceMetadataRequester
{
    private readonly IEnumerable<IMetadataRequester> _requesters;

    public SourceMetadataRequester(IEnumerable<IMetadataRequester> requesters) => _requesters = requesters;
    
    public Task Request(MetadataSource source, Guid fileId, string md5) => Get(source).SendRequestCommand(fileId, md5);

    private IMetadataRequester Get(MetadataSource source)
    {
        var requester = _requesters.FirstOrDefault(x => x.Source == source);

        if (requester == null)
            throw new NotImplementedException($"Metadata requester for source {source} was not found.");

        return requester;
    }
}
