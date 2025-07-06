namespace ImoutoRebirth.Room.Application.Services;

public interface IRemoteCommandService
{
    Task UpdateMetadataRequest(Guid fileId, string md5, string fileName);

    Task SaveTags(Guid fileId, IReadOnlyCollection<string> tags);

    Task UpdateFileMetadataRequest(Guid fileId, string fullName);
}
