using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class FileTagService : IFileTagService
{
    private readonly FilesClient _filesClient;
    private readonly ITagService _tagService;
    private readonly IMapper _mapper;

    public FileTagService(
        IMapper mapper, 
        ITagService tagService,
        FilesClient filesClient)
    {
        _tagService = tagService;
        _filesClient = filesClient;
        _mapper = mapper;
    }

    public async Task SetRate(Guid fileId, Rate rate)
    {
        var rateTag = await GetOrCreateTag("Rate", "LocalMeta", true);

        await _filesClient.BindTagsToFilesAsync(
            new BindTagsRequest(
                new List<FileTagRequest>
                {
                    new(fileId, FileTagRequestSource.Manual, rateTag.Id, rate.Rating.ToString())
                },
                BindTagsRequestSameTagHandleStrategy.ReplaceExistingValue));
    }

    public async Task SetFavorite(Guid fileId, bool value)
    {
        var favTag = await GetOrCreateTag("Favorite", "LocalMeta", false);


        if (value)
        {
            await _filesClient.BindTagsToFilesAsync(
                new BindTagsRequest(
                    new List<FileTagRequest>
                    {
                        new(fileId, FileTagRequestSource.Manual, favTag.Id, default)
                    },
                    BindTagsRequestSameTagHandleStrategy.ReplaceExistingValue));
        }
        else
        {
            await _filesClient.UnbindTagFromFileAsync(
                new UnbindTagRequest(new FileTagRequest(fileId, FileTagRequestSource.Manual,
                    favTag.Id, default)));
        }
    }

    public async Task BindTags(IReadOnlyCollection<FileTag> fileTags)
    {
        var requests = _mapper.Map<IReadOnlyCollection<FileTagRequest>>(fileTags);

        await _filesClient.BindTagsToFilesAsync(
            new BindTagsRequest(requests, BindTagsRequestSameTagHandleStrategy.AddNewFileTag));
    }

    public async Task UnbindTag(Guid fileId, Guid tagId, FileTagSource source)
    {
        var metadataSource = _mapper.Map<FileTagRequestSource>(source);

        await _filesClient.UnbindTagFromFileAsync(
            new UnbindTagRequest(new FileTagRequest(fileId, FileTagRequestSource.Manual, tagId, default)));
    }

    public async Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId)
    {
        var info = await _filesClient.GetFileInfoAsync(fileId);

        return _mapper.Map<IReadOnlyCollection<FileTag>>(info.Tags);
    }

    private async Task<Tag> GetOrCreateTag(string name, string typeName, bool hasValue)
    {
        var rateTags = await _tagService.SearchTags(name, 1);
        var rateTag = rateTags.FirstOrDefault();

        if (rateTag?.Title != name || rateTag?.HasValue != hasValue || rateTag.Type.Title != typeName)
        {
            rateTag = null;
        }

        if (rateTag != null)
        {
            return rateTag;
        }

        var types = await _tagService.GеtTypes();
        var localType = types.First(x => x.Title == typeName);
        await _tagService.CreateTag(localType.Id, name, hasValue, Array.Empty<string>());

        rateTags = await _tagService.SearchTags(name, 1);
        rateTag = rateTags.First();

        return rateTag;
    }
}
