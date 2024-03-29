﻿using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class FileService : IFileService
{
    private readonly FilesClient _filesClient;
    private readonly IMapper _mapper;
    private readonly CollectionFilesClient _collectionFilesClient;

    public FileService(
        IMapper mapper,
        FilesClient filesClient,
        CollectionFilesClient collectionFilesClient)
    {
        _mapper = mapper;
        _filesClient = filesClient;
        _collectionFilesClient = collectionFilesClient;
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(string md5, CancellationToken ct)
    {
        var files = await _collectionFilesClient.SearchCollectionFilesAsync(
            new(default, default, int.MaxValue, new[] { md5 }, default, default), ct);
        
        return _mapper.Map<IReadOnlyCollection<File>>(files);
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        CancellationToken ct)
    {
        if (!tags.Any())
        {
            var filesOnly = await _collectionFilesClient
                .SearchCollectionFilesAsync(new(default, collectionId, int.MaxValue, default, default, 0), ct);

            return _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
        }

        var roomFiles = await _collectionFilesClient
            .SearchCollectionFilesAsync(new(default, collectionId, int.MaxValue, default, default, 0), ct);

        var roomFilesIds = roomFiles.Select(x => x.Id).ToList();
        
        var lilinFilesThatSatisfyConditions = await _filesClient
            .FilterFilesAsync(
                new FilterFilesQuery(
                    roomFilesIds,
                    _mapper.Map<List<TagSearchEntry>>(tags)),
                ct);

        var lilinFilesThatSatisfyConditionsHashSet = lilinFilesThatSatisfyConditions.ToHashSet();

        var satisfiedRoomFiles = roomFiles.Where(x => lilinFilesThatSatisfyConditionsHashSet.Contains(x.Id)).ToList();
        
        return _mapper.Map<IReadOnlyCollection<File>>(satisfiedRoomFiles);
    }

    public async Task<int> CountFiles(
        Guid? collectionId,
        IReadOnlyCollection<SearchTag> tags,
        CancellationToken ct)
    {
        if (!tags.Any())
        {
            var result = await _collectionFilesClient
                .CountCollectionFilesAsync(new(default, collectionId, default, default, default, default), ct);

            return result;
        }

        return await _filesClient
            .CountSearchFilesFastAsync(new SearchFilesFastCountQuery(_mapper.Map<List<TagSearchEntry>>(tags)), ct);
    }
}
