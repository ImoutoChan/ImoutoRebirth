using ImoutoRebirth.Lamia.Application.Services;
using ImoutoRebirth.Lamia.Domain.FileAggregate;
using ImoutoRebirth.Lamia.Infrastructure.Meta;

namespace ImoutoRebirth.Lamia.Infrastructure;

internal class MetadataProvider : IMetadataProvider
{
    private readonly IEnumerable<IMetadataForFileProvider> _providers;

    public MetadataProvider(IEnumerable<IMetadataForFileProvider> providers) => _providers = providers;

    public Task<FileMetadata> GetMetadata(string filePath)
    {
        foreach (var provider in _providers)
        {
            if (provider.IsProviderFor(filePath))
                return provider.GetMetadata(filePath);
        }

        throw new NotSupportedException($"File format isn't supported: {filePath}");
    }
}
