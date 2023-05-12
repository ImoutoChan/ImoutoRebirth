namespace ImoutoRebirth.Room.Core.Services.Abstract;

public interface IRemoteCommandService
{
    Task UpdateMetadataRequest(Guid fileId, string md5);

    Task SaveTags(Guid fileId, IReadOnlyCollection<string> tags);
}