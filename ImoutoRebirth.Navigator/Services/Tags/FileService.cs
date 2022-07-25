using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Tags;

class FileService : IFileService
{
    private readonly FilesClient _filesClient;
    private readonly IMapper _mapper;
    private readonly IImoutoRebirthRoomWebApiClient _roomClient;

    public FileService(
        IImoutoRebirthRoomWebApiClient roomClient,
        IMapper mapper,
        FilesClient filesClient)
    {
        _roomClient = roomClient;
        _mapper = mapper;
        _filesClient = filesClient;
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
            var filesOnly = await _roomClient.CollectionFiles
                .SearchAsync(new CollectionFilesRequest(collectionId, count: take, skip: skip), token);

            return _mapper.Map<IReadOnlyCollection<File>>(filesOnly);
        }

        var tagsSearch = await _filesClient
            .GetFilesByTagsAsync(
                new FilesSearchRequest(
                    take, skip,
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                token);

        var filesSearch = await _roomClient.CollectionFiles
            .SearchAsync(
                new CollectionFilesRequest(collectionId, tagsSearch.Cast<Guid?>().ToList()),
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
            var result = await _roomClient.CollectionFiles
                .CountAsync(new CollectionFilesRequest(collectionId), cancellationToken);

            return result.Value;
        }

        return await _filesClient
            .GetFilesCountByTagsAsync(
                new FilesSearchRequest(
                    int.MaxValue,
                    0,
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken);
    }

    public Task RemoveFile(Guid fileId) => _roomClient.CollectionFiles.RemoveAsync(fileId);
}
