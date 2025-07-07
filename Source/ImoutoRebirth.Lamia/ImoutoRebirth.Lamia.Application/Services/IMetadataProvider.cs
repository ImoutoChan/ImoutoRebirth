using ImoutoRebirth.Lamia.Domain.FileAggregate;

namespace ImoutoRebirth.Lamia.Application.Services;

public interface IMetadataProvider
{
    public Task<FileMetadata> GetMetadata(string filePath);
}
