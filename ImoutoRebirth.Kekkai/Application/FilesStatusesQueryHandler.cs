using System.Runtime.CompilerServices;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Kekkai.Application;

internal class FilesStatusesQueryHandler : IQueryHandler<FilesStatusesQuery, IAsyncEnumerable<FileStatusResult>>
{
    private readonly FilesClient _filesLilinClient;
    private readonly IImoutoRebirthRoomWebApiClient _roomWebApiClient;

    public FilesStatusesQueryHandler(FilesClient filesLilinClient, IImoutoRebirthRoomWebApiClient roomWebApiClient)
    {
        _filesLilinClient = filesLilinClient;
        _roomWebApiClient = roomWebApiClient;
    }

    public Task<IAsyncEnumerable<FileStatusResult>> Handle(FilesStatusesQuery request, CancellationToken ct)
    {
        return Task.FromResult(LoadAsync(request, ct));
    }

    private async IAsyncEnumerable<FileStatusResult> LoadAsync(
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
        var loaded =
            await _roomWebApiClient.CollectionFiles.SearchAsync(
                new CollectionFilesRequest(md5: hashes.ToList()), ct);

        var loadedHashset = loaded.Select(x => x.Md5).ToHashSet();

        return hashes
            .ToDictionary(
                hash => hash,
                hash => loadedHashset.Contains(hash) ? FileStatus.Present : FileStatus.NotFound);
    }

    private async Task<IReadOnlyDictionary<string, FileStatus>> GetFromLilinAsync(
        IReadOnlyCollection<string> hashes,
        CancellationToken ct)
    {
        var response = await _filesLilinClient.GetRelativesBatchAsync(hashes, ct);

        return response.ToDictionary(
            x => x.Hash!,
            x => x.RelativeType == null
                ? FileStatus.NotFound
                : FileStatus.RelativePresent);
    }
}
