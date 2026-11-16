using System.Collections.Concurrent;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface IRoomCache
{
    Task<IReadOnlyCollection<File>> GetFilesFromCollection(
        Guid? collectionId,
        int? skip,
        int? take,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<File>> GetFilesByIds(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Guid>> GetIds(
        Guid? collectionId,
        int? skip,
        int? take,
        CancellationToken ct = default);

    void Clear();
}

internal class RoomCache : IRoomCache
{
    private static readonly ConcurrentDictionary<Guid, File> Files = new();
    private static CancellationTokenSource _resetCacheToken = new();

    private readonly CollectionFilesClient _collectionFilesClient;
    private readonly IMemoryCache _memoryCache;

    public RoomCache(CollectionFilesClient collectionFilesClient, IMemoryCache memoryCache)
    {
        _collectionFilesClient = collectionFilesClient;
        _memoryCache = memoryCache;
    }

    public async Task<IReadOnlyCollection<Guid>> GetIds(
        Guid? collectionId,
        int? skip,
        int? take,
        CancellationToken ct = default)
    {
        var key = GetIdsKey(collectionId, skip, take);
        var requestUsed = false;
        var ids = await _memoryCache.GetOrCreateAsync(
            key,
            async entry =>
            {
                var loaded = await _collectionFilesClient.SearchCollectionFileIdsAsync(
                    new(default, collectionId, take, default, default, skip),
                    ct);
                requestUsed = true;
                entry.AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));
                return loaded;
            }) ?? Array.Empty<Guid>();

        if (requestUsed || take.HasValue && ids.Count >= take.Value)
            return ids;

        var newIds = await _collectionFilesClient.SearchCollectionFileIdsAsync(
            new(default, collectionId, take - ids.Count, default, default, skip.GetValueOrDefault() + ids.Count),
            ct);
        if (newIds.Count == 0)
            return ids;

        ids = ids.Union(newIds).ToArray();
        CacheIds(key, ids);
        return ids;
    }

    public void Clear()
    {
        if (_resetCacheToken is { IsCancellationRequested: false, Token.CanBeCanceled: true })
        {
            _resetCacheToken.Cancel();
            _resetCacheToken.Dispose();
        }

        _resetCacheToken = new CancellationTokenSource();
        Files.Clear();
    }

    public async Task<IReadOnlyCollection<File>> GetFilesFromCollection(
        Guid? collectionId,
        int? skip,
        int? take,
        CancellationToken ct = default)
    {
        var key = GetIdsKey(collectionId, skip, take);
        if (_memoryCache.TryGetValue(key, out IReadOnlyCollection<Guid>? _))
            return await GetFilesByIds(await GetIds(collectionId, skip, take, ct), ct);

        var loaded = await _collectionFilesClient.SearchCollectionFileSummariesAsync(
            new(default, collectionId, take, default, default, skip),
            ct);
        var files = loaded.Select(x => new File(x.Id, x.Path)).ToArray();
        foreach (var file in files)
            Files.TryAdd(file.Id, file);

        CacheIds(key, files.Select(x => x.Id).ToArray());
        return files;
    }

    public async Task<IReadOnlyCollection<File>> GetFilesByIds(
        IReadOnlyCollection<Guid> ids,
        CancellationToken ct = default)
    {
        var newFileIds = ids.Distinct().Where(x => !Files.ContainsKey(x)).ToArray();
        if (newFileIds.Length != 0)
        {
            var loaded = await _collectionFilesClient.SearchCollectionFileSummariesAsync(
                new(newFileIds, default, default, default, default, default),
                ct);
            foreach (var file in loaded)
                Files.TryAdd(file.Id, new File(file.Id, file.Path));
        }

        var result = new List<File>(ids.Count);
        foreach (var id in ids)
        {
            if (Files.TryGetValue(id, out var file))
                result.Add(file);
        }
        return result;
    }

    private void CacheIds(string key, IReadOnlyCollection<Guid> ids)
        => _memoryCache.Set(key, ids, new CancellationChangeToken(_resetCacheToken.Token));

    private static string GetIdsKey(Guid? collectionId, int? skip, int? take)
        => $"collection_file_ids_{collectionId}_{skip.GetValueOrDefault()}_{(take == int.MaxValue ? null : take)}";
}