using ImoutoRebirth.Lamia.Application.Services;
using ImoutoRebirth.Lamia.Domain.FileAggregate;
using ImoutoRebirth.Lamia.Infrastructure.Meta;

namespace ImoutoRebirth.Lamia.Infrastructure;

internal class MetadataProvider : IMetadataProvider
{
    private readonly IEnumerable<IMetadataForFileProvider> _providers;

    public MetadataProvider(IEnumerable<IMetadataForFileProvider> providers) => _providers = providers;

    public async Task<FileMetadata?> GetMetadata(string filePath)
    {
        var provider = _providers.FirstOrDefault(x => x.IsProviderFor(filePath));
        return provider is null ? null : await provider.GetMetadata(filePath);
    }
}
