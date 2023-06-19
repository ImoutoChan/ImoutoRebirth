using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class FileService : IFileService
{
    private readonly FilesClient _filesClient;
    private readonly IMapper _mapper;
    private readonly CollectionFilesClient _collectionFilesClient;
    private readonly IRoomCache _roomCache;

    public FileService(
        IMapper mapper,
        FilesClient filesClient,
        CollectionFilesClient collectionFilesClient,
        IRoomCache roomCache)
    {
        _mapper = mapper;
        _filesClient = filesClient;
        _collectionFilesClient = collectionFilesClient;
        _roomCache = roomCache;
    }

    public async Task<(IReadOnlyCollection<File> Files, bool Continue)> SearchFilesV1(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        int take,
        int skip,
        CancellationToken token)
    {
        if (!tags.Any())
        {
            var filesOnly = await _collectionFilesClient
                .SearchAsync(new CollectionFilesRequest(default, collectionId, take, default, default, skip), token);

            var filesMapped = _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
            return (filesMapped, filesOnly.Any());
        }

        var roomFiles = await _collectionFilesClient
            .SearchAsync(new CollectionFilesRequest(default, collectionId, take, default, default, skip), token);

        var roomFilesIds = roomFiles.Select(x => x.Id).ToList();
        
        var lilinFilesThatSatisfyConditions = await _filesClient
            .FilterFilesAsync(
                new FilterFilesQuery(
                    roomFilesIds,
                    _mapper.Map<List<TagSearchEntry>>(tags)),
                token);

        var lilinFilesThatSatisfyConditionsHashSet = lilinFilesThatSatisfyConditions.ToHashSet();

        var satisfiedRoomFiles =
            roomFiles.Where(x => lilinFilesThatSatisfyConditionsHashSet.Contains(x.Id)).ToList();

        var files = _mapper.Map<IReadOnlyCollection<File>>(satisfiedRoomFiles);

        return (files, roomFilesIds.Any());
    }
    
    public async Task<(IReadOnlyCollection<File> Files, bool Continue)> SearchFilesV2(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        int take,
        int skip,
        CancellationToken token)
    {
        if (!tags.Any())
        {
            var filesMapped = await _roomCache.GetFilesFromCollection(collectionId, skip, take);
            return (filesMapped, filesMapped.Any());
        }

        var roomFilesIds = await _roomCache.GetIds(collectionId, skip, take);
        
        var lilinFilesThatSatisfyConditions = await _filesClient
            .FilterFilesAsync(
                new FilterFilesQuery(
                    roomFilesIds,
                    _mapper.Map<List<TagSearchEntry>>(tags)),
                token);

        var files = await _roomCache.GetFilesByIds(lilinFilesThatSatisfyConditions);

        return (files, roomFilesIds.Any());
    }
    
    public async Task<(IReadOnlyCollection<File> Files, bool Continue)> SearchFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        int take,
        int skip,
        CancellationToken token)
    {
        if (!tags.Any())
        {
            var filesMapped = await _roomCache.GetFilesFromCollection(collectionId, skip, take);
            return (filesMapped, filesMapped.Any());
        }

        // skip and take isn't supported
        var lilinIds = await _filesClient
            .SearchFilesFastAsync(
                new SearchFilesFastQuery(_mapper.Map<List<TagSearchEntry>>(tags)),
                token);
        var lilinIdsHashSet = lilinIds.ToHashSet();

        var roomIds = await _roomCache.GetIds(collectionId, default, default);
        var files = await _roomCache.GetFilesByIds(roomIds.Where(x =>  lilinIdsHashSet.Contains(x)).ToList());

        return (files, files.Any() && take != int.MaxValue);
    }

    public async Task<int> CountFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        CancellationToken ct)
    {
        if (!tags.Any())
        {
            var result = await _collectionFilesClient
                .CountAsync(new CollectionFilesRequest(default, collectionId, default, default, default, default), ct);

            return result;
        }

        return await _filesClient
            .CountSearchFilesAsync(new SearchFilesCountQuery(_mapper.Map<List<TagSearchEntry>>(tags)), ct);
    }

    public Task RemoveFile(Guid fileId) => _collectionFilesClient.RemoveAsync(fileId);
}
