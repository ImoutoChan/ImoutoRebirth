using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class FileTagService : IFileTagService
{
    private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
    private readonly ITagService _tagService;
    private readonly IMapper _mapper;

    public FileTagService(
        IImoutoRebirthLilinWebApiClient lilinClient, 
        IMapper mapper, 
        ITagService tagService)
    {
        _lilinClient = lilinClient;
        _tagService = tagService;
        _mapper = mapper;
    }

    public async Task SetRate(Guid fileId, Rate rate)
    {
        var rateTag = await GetOrCreateTag("Rate", "LocalMeta", true);

        await _lilinClient.Files.BindTagsToFilesAsync(
            new BindTagsRequest(
                new List<FileTagRequest>
                {
                    new FileTagRequest(rateTag.Id, fileId, MetadataSource.Manual, rate.Rating.ToString())
                },
                SameTagHandleStrategy.ReplaceExistingValue));
    }

    public async Task SetFavorite(Guid fileId, bool value)
    {
        var favTag = await GetOrCreateTag("Favorite", "LocalMeta", false);


        if (value)
        {
            await _lilinClient.Files.BindTagsToFilesAsync(
                new BindTagsRequest(
                    new[] {new FileTagRequest(favTag.Id, fileId, MetadataSource.Manual)},
                    SameTagHandleStrategy.ReplaceExistingValue));
        }
        else
        {
            await _lilinClient.Files.UnbindTagFromFileAsync(
                new UnbindTagRequest(new UnbindTagRequestFileTag(favTag.Id, fileId, MetadataSource.Manual)));
        }
    }

    public async Task BindTags(IReadOnlyCollection<FileTag> fileTags)
    {
        var requests = _mapper.Map<IList<FileTagRequest>>(fileTags);

        await _lilinClient.Files.BindTagsToFilesAsync(
            new BindTagsRequest(requests, SameTagHandleStrategy.AddNewFileTag));
    }

    public async Task UnbindTag(Guid fileId, Guid tagId, FileTagSource source)
    {
        var metadataSource = _mapper.Map<MetadataSource>(source);

        await _lilinClient.Files.UnbindTagFromFileAsync(
            new UnbindTagRequest(new UnbindTagRequestFileTag(tagId, fileId, metadataSource)));
    }

    public async Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId)
    {
        var info = await _lilinClient.Files.GetFileInfoAsync(fileId);

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