using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using ImoutoRebirth.Room.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client.Models;

namespace ImoutoRebirth.Navigator.Services.Tags;

class FileService : IFileService
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

        var tagsSearch = await _lilinClient.Files
            .GetFilesByTagsAsync(
                new FilesSearchRequest(
                    _mapper.Map<List<TagSearchEntryRequest>>(tags), take, skip),
                token);

        var filesSearch = await _roomClient.CollectionFiles
            .SearchAsync(new CollectionFilesRequest(collectionId, tagsSearch), token);

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

        return (await _lilinClient.Files
            .GetFilesCountByTagsAsync(
                new FilesSearchRequest(
                    _mapper.Map<List<TagSearchEntryRequest>>(tags)),
                cancellationToken)).Value;
    }

    public Task RemoveFile(Guid fileId) => _roomClient.CollectionFiles.RemoveAsync(fileId);
}
