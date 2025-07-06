using ImoutoRebirth.Lamia.Domain.FileAggregate;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta;

internal interface IMetadataForFileProvider
{
    bool IsProviderFor(string filePath);

    Task<FileMetadata> GetMetadata(string filePath);
}
