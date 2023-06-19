using System.Collections.Concurrent;
using AutoMapper;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.RoomService.WebApi.Client;
using Microsoft.Extensions.Caching.Memory;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface IRoomCache
{
    Task<IReadOnlyCollection<File>> GetFilesFromCollection(Guid? collectionId, int? skip, int? take);

    Task<IReadOnlyCollection<File>> GetFilesByIds(IReadOnlyCollection<Guid> ids);
    
    Task<IReadOnlyCollection<Guid>> GetIds(Guid? collectionId, int? skip, int? take);
}

internal class RoomCache : IRoomCache
{
    private static readonly ConcurrentDictionary<Guid, File> Files = new();

    private readonly CollectionFilesClient _collectionFilesClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IMapper _mapper;

    public RoomCache(CollectionFilesClient collectionFilesClient, IMemoryCache memoryCache, IMapper mapper)
    {
        _collectionFilesClient = collectionFilesClient;
        _memoryCache = memoryCache;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<Guid>> GetIds(Guid? collectionId, int? skip, int? take)
    {
        var key = "collection_file_ids_" + collectionId + "_" + skip + "_" + take;

        var requestUsed = false;
        var ids = await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            var ids = await _collectionFilesClient.SearchIdsAsync(new CollectionFilesRequest(
                default,
                collectionId,
                take,
                default,
                default,
                skip));

            requestUsed = true;

            return ids;
        }) ?? Array.Empty<Guid>();

        if (requestUsed || ids.Count >= take) 
            return ids;
        
        var newIds = await _collectionFilesClient.SearchIdsAsync(new CollectionFilesRequest(
            default,
            collectionId,
            take - ids.Count,
            default,
            default,
            skip + ids.Count));

        ids = ids.Union(newIds).ToList();

        return ids;
    }

    public async Task<IReadOnlyCollection<File>> GetFilesFromCollection(Guid? collectionId, int? skip, int? take)
    {
        var ids = await GetIds(collectionId, skip, take);
        return await GetFilesByIds(ids);
    }

    public async Task<IReadOnlyCollection<File>> GetFilesByIds(IReadOnlyCollection<Guid> ids)
    {
        var idsHash = ids.ToHashSet();
        
        var newFileIds = idsHash.Where(x => !Files.ContainsKey(x)).ToList();

        if (newFileIds.Any())
        {
            var newFiles = await _collectionFilesClient.SearchAsync(new CollectionFilesRequest(
                newFileIds,
                default,
                default,
                default,
                default,
                default));

            foreach (var newFile in newFiles)
                Files.TryAdd(newFile.Id, _mapper.Map<File>(newFile));
        }

        return GetFiles(ids).ToList();

        static IEnumerable<File> GetFiles(IReadOnlyCollection<Guid> fileIds)
        {
            foreach (var id in fileIds)
            {
                if (Files.TryGetValue(id, out var file))
                    yield return file;
            }
        }
    }
}
