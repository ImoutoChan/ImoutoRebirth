using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Kekkai.Application;

internal class FilesStatusesQueryHandler : IQueryHandler<FilesStatusesQuery, IReadOnlyCollection<FileStatusResult>>
{
    private readonly IImoutoRebirthLilinWebApiClient _lilinWebApiClient;
    private readonly IImoutoRebirthRoomWebApiClient _roomWebApiClient;

    public FilesStatusesQueryHandler(
        IImoutoRebirthRoomWebApiClient roomWebApiClient,
        IImoutoRebirthLilinWebApiClient lilinWebApiClient)
    {
        _roomWebApiClient = roomWebApiClient;
        _lilinWebApiClient = lilinWebApiClient;
    }

    public async Task<IReadOnlyCollection<FileStatusResult>> Handle(FilesStatusesQuery request, CancellationToken ct)
    {
        var roomFilesTask = GetFromRoomAsync(request.Hashes, ct);

        var lilinFilesTask = GetFromLilinAsync(request.Hashes, ct);
        await Task.WhenAll(roomFilesTask, lilinFilesTask);

        var lilinFiles = lilinFilesTask.Result;

        return roomFilesTask.Result
            .Select(x => new FileStatusResult(
                x.Key,
                x.Value != FileStatus.NotFound ? x.Value : lilinFiles[x.Key]))
            .ToList();
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
        var tasks = hashes.Select(x => (Hash: x, Task: _lilinWebApiClient.Files.GetRelativesAsync(x, ct))).ToList();

        await Task.WhenAll(tasks.Select(x => x.Task));

        return tasks.ToDictionary(
            x => x.Hash,
            x => x.Task.Result.Any() ? FileStatus.RelativePresent : FileStatus.NotFound);
    }
}