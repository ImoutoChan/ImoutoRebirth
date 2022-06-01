using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class FileService : IFileService
{
    private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
    private readonly IMapper _mapper;
    private readonly IImoutoRebirthRoomWebApiClient _roomClient;

    public FileService(
        IImoutoRebirthLilinWebApiClient lilinClient,
        IImoutoRebirthRoomWebApiClient roomClient,
        IMapper mapper)
    {
        _lilinClient = lilinClient;
        _roomClient = roomClient;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(string md5, CancellationToken token)
    {
        var filesOnly = await _roomClient.CollectionFiles
            .SearchAsync(new CollectionFilesRequest(md5: new [] { md5 }), token);

        return _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(
        Guid? collectionId, 
        IReadOnlyCollection<SearchTag> tags, 
        CancellationToken cancellationToken)
    {
        if (!tags.Any())
        {
            var result = await _roomClient.CollectionFiles
                .SearchAsync(new CollectionFilesRequest(collectionId), cancellationToken);

            if (result == null)
                return ArraySegment<File>.Empty;
                
            return _mapper.Map<IReadOnlyCollection<File>>(result);
        }

        var files = await _lilinClient.Files
            .GetFilesByTagsAsync(
                new FilesSearchRequest(
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken);
            
        if (files == null || !files.Any())
            return ArraySegment<File>.Empty;

        var collectionFiles = await _roomClient.CollectionFiles
            .SearchAsync(new CollectionFilesRequest(collectionId, files), cancellationToken);
            
        if (collectionFiles == null || !collectionFiles.Any())
            return ArraySegment<File>.Empty;
            
        return _mapper.Map<IReadOnlyCollection<File>>(collectionFiles);
    }

    public async Task<int> CountFiles(
        Guid? collectionId, 
        IReadOnlyCollection<SearchTag> tags,
        CancellationToken cancellationToken)
    {
        if (!tags.Any())
        {
            var result = await _roomClient.CollectionFiles
                .CountAsync(new CollectionFilesRequest(collectionId), cancellationToken);

            return result ?? 0;
        }

        return (await _lilinClient.Files
            .GetFilesCountByTagsAsync(
                new FilesSearchRequest(
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken)) ?? 0;
    }

    public Task RemoveFile(Guid fileId)
    {
        throw new NotImplementedException();
    }
}