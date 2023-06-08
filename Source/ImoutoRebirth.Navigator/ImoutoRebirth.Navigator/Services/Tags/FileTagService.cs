using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;
using FileTag = ImoutoRebirth.Navigator.Services.Tags.Model.FileTag;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;

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

        await _filesClient.BindTagsAsync(
            new BindTagsCommand(
                new List<FileTagInfo>
                {
                    new(fileId, MetadataSource.Manual, rateTag.Id, rate.Rating.ToString())
                },
                SameTagHandleStrategy.ReplaceExistingValue));
    }

    public async Task SetFavorite(Guid fileId, bool value)
    {
        var favTag = await GetOrCreateTag("Favorite", "LocalMeta", false);


        if (value)
        {
            await _filesClient.BindTagsAsync(
                new BindTagsCommand(
                    new List<FileTagInfo>
                    {
                        new(fileId, MetadataSource.Manual, favTag.Id, default)
                    },
                    SameTagHandleStrategy.ReplaceExistingValue));
        }
        else
        {
            await _filesClient.UnbindTagsAsync(
                new UnbindTagCommand(new FileTagInfo(fileId, MetadataSource.Manual,
                    favTag.Id, default)));
        }
    }

    public async Task BindTags(IReadOnlyCollection<FileTag> fileTags)
    {
        var requests = _mapper.Map<IReadOnlyCollection<FileTagInfo>>(fileTags);

        await _filesClient.BindTagsAsync(
            new BindTagsCommand(requests, SameTagHandleStrategy.AddNewFileTag));
    }

    public async Task UnbindTag(Guid fileId, Guid tagId, FileTagSource source)
    {
        await _filesClient.UnbindTagsAsync(
            new UnbindTagCommand(new FileTagInfo(fileId, MetadataSource.Manual, tagId, default)));
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
