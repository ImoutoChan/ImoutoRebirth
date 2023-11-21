using System.Runtime.CompilerServices;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Kekkai.Application;

public record FilesStatusesQuery(IReadOnlyCollection<string> Hashes) : IStreamQuery<FileStatusResult>;

public record FileStatusResult(string Hash, FileStatus Status);

public enum FileStatus
{
    NotFound,
    Present,
    RelativePresent
}

internal class FilesStatusesQueryHandler : IStreamQueryHandler<FilesStatusesQuery, FileStatusResult>
{
    private readonly FilesClient _filesLilinClient;
    private readonly CollectionFilesClient _collectionFilesClient;

    public FilesStatusesQueryHandler(FilesClient filesLilinClient, CollectionFilesClient collectionFilesClient)
    {
        _filesLilinClient = filesLilinClient;
        _collectionFilesClient = collectionFilesClient;
    }

    public async IAsyncEnumerable<FileStatusResult> Handle(
        FilesStatusesQuery request,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var roomFiles = await GetFromRoomAsync(request.Hashes, ct);

        foreach (var roomFile in roomFiles.Where(x => x.Value != FileStatus.NotFound))
            yield return new FileStatusResult(roomFile.Key, roomFile.Value);

        var notFoundInRoom = roomFiles.Where(x => x.Value == FileStatus.NotFound).Select(x => x.Key).ToList();

        if (!notFoundInRoom.Any())
            yield break;

        var lilinFiles = await GetFromLilinAsync(notFoundInRoom, ct);

        foreach (var lilinFile in lilinFiles)
            yield return new FileStatusResult(lilinFile.Key, lilinFile.Value);
    }

    private async Task<IReadOnlyDictionary<string, FileStatus>> GetFromRoomAsync(
        IReadOnlyCollection<string> hashes,
        CancellationToken ct)
    {
        var loaded = await _collectionFilesClient.FilterCollectionFileHashesAsync(new(hashes), ct);

        var loadedHashset = loaded.ToHashSet();

        return hashes
            .ToDictionary(
                hash => hash,
                hash => loadedHashset.Contains(hash) ? FileStatus.Present : FileStatus.NotFound);
    }

    private async Task<IReadOnlyDictionary<string, FileStatus>> GetFromLilinAsync(
        IReadOnlyCollection<string> hashes,
        CancellationToken ct)
    {
        var response = await _filesLilinClient.GetRelativesInfoBatchAsync(hashes, ct);

        return response.ToDictionary(
            x => x.Hash!,
            x => x.RelativeType == null
                ? FileStatus.NotFound
                : FileStatus.RelativePresent);
    }
}
