using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.RoomService.WebApi.Client;

namespace ImoutoRebirth.Navigator.Services.Tags;

class FileService : IFileService
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

    public async Task<IReadOnlyCollection<File>> SearchFiles(
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

            return _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
        }

        var tagsSearch = await _filesClient
            .GetFilesByTagsAsync(
                new FilesSearchRequest(
                    take, skip,
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                token);

        var filesSearch = await _collectionFilesClient
            .SearchAsync(
                new CollectionFilesRequest(tagsSearch.ToList(), collectionId, default, default, default, default),
                token);

        return _mapper.Map<IReadOnlyCollection<File>>(filesSearch);
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
                    int.MaxValue,
                    0,
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken);
    }

    public Task RemoveFile(Guid fileId) => _collectionFilesClient.RemoveAsync(fileId);
}
