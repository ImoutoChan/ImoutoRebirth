using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Kekkai.Application;

internal class FilesStatusesQueryHandler : IQueryHandler<FilesStatusesQuery, IReadOnlyCollection<FileStatusResult>>
{
    private readonly FilesClient _filesLilinClient;
    private readonly IImoutoRebirthRoomWebApiClient _roomWebApiClient;

    public FilesStatusesQueryHandler(FilesClient filesLilinClient, IImoutoRebirthRoomWebApiClient roomWebApiClient)
    {
        _filesLilinClient = filesLilinClient;
        _roomWebApiClient = roomWebApiClient;
    }

    public async Task<IReadOnlyCollection<FileStatusResult>> Handle(FilesStatusesQuery request, CancellationToken ct)
    {
        var roomFiles = await GetFromRoomAsync(request.Hashes, ct);
        var notFoundInRoom = roomFiles.Where(x => x.Value == FileStatus.NotFound).Select(x => x.Key).ToList();

        var lilinFiles = await GetFromLilinAsync(notFoundInRoom, ct);

        return roomFiles
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
        var response = await _filesLilinClient.GetRelativesBatchAsync(hashes, ct);

        return response.ToDictionary(
            x => x.Hash!,
            x => x.RelativeType == null
                ? FileStatus.NotFound
                : FileStatus.RelativePresent);
    }
}
