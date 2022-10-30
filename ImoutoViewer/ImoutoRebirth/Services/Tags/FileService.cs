using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.RoomService.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class FileService : IFileService
{
    private readonly IMapper _mapper;
    private readonly CollectionFilesClient _collectionFilesClient;
    private readonly FilesClient _filesClient;

    public FileService(IMapper mapper, CollectionFilesClient collectionFilesClient, FilesClient filesClient)
    {
        _mapper = mapper;
        _collectionFilesClient = collectionFilesClient;
        _filesClient = filesClient;
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(string md5, CancellationToken token)
    {
        var filesOnly = await _collectionFilesClient
            .SearchAsync(new CollectionFilesRequest(default, default, 1, new[] { md5 }, default, 0), token);

        return _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
    }

    public async Task<IReadOnlyCollection<File>> SearchFiles(
        Guid? collectionId, 
        IReadOnlyCollection<SearchTag> tags, 
        CancellationToken cancellationToken)
    {
        if (!tags.Any())
        {
            var result = await _collectionFilesClient
                .SearchAsync(
                    new CollectionFilesRequest(default, collectionId, default, default, default, default),
                    cancellationToken);

            if (result == null)
                return ArraySegment<File>.Empty;
                
            return _mapper.Map<IReadOnlyCollection<File>>(result);
        }

        var files = await _filesClient
            .GetFilesByTagsAsync(
                new FilesSearchRequest(
                    default, 
                    default,
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken);
            
        if (files == null || !files.Any())
            return ArraySegment<File>.Empty;

        var collectionFiles = await _collectionFilesClient
            .SearchAsync(new CollectionFilesRequest(files, collectionId, default, default, default, default),
                cancellationToken);
            
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
            var result = await _collectionFilesClient
                .CountAsync(new CollectionFilesRequest(default, collectionId, default, default, default, default), cancellationToken);

            return result;
        }

        return await _filesClient
            .GetFilesCountByTagsAsync(
                new FilesSearchRequest(
                    default, 
                    default, 
                    _mapper.Map<IReadOnlyCollection<TagSearchEntryRequest>>(tags)),
                cancellationToken);
    }

    public Task RemoveFile(Guid fileId)
    {
        throw new NotImplementedException();
    }
}
