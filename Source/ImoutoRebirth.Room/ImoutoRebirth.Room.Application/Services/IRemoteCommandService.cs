namespace ImoutoRebirth.Room.Application.Services;

public interface IRemoteCommandService
{
    Task RequestMetadataUpdate(Guid fileId, string md5, string fileName);

    Task RequestMetadataUpdateForRenamedFile(Guid fileId, string md5, string newFileName);

    Task SaveLocationTags(Guid fileId, IReadOnlyCollection<string> tags);

    Task UpdateFileMetadataRequest(Guid fileId, string fullName);
}
