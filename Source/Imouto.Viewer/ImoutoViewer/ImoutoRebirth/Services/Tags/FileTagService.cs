using AutoMapper;
using ImoutoRebirth.Common;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;
using FileNote = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.FileNote;
using FileTag = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.FileTag;
using Tag = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.Tag;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

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
                new List<BindTag>
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
                    new List<BindTag>
                    {
                        new(fileId, MetadataSource.Manual, favTag.Id, default)
                    },
                    SameTagHandleStrategy.ReplaceExistingValue));
        }
        else
        {
            await _filesClient.UnbindTagsAsync(
                new UnbindTagsCommand(new BindTag(fileId, MetadataSource.Manual, favTag.Id, default).AsArray()));
        }
    }

    public async Task BindTags(IReadOnlyCollection<FileTag> fileTags)
    {
        var requests = fileTags.Select(tag => new BindTag(tag.FileId, _mapper.Map<MetadataSource>(tag.Source), tag.Tag.Id, tag.Value)).ToList();

        await _filesClient.BindTagsAsync(
            new BindTagsCommand(requests, SameTagHandleStrategy.AddNewFileTag));
    }

    public async Task UnbindTags(params UnbindTagRequest[] tagsToUnbind)
    {
        var tags = tagsToUnbind
            .Select(x => new BindTag(x.FileId, _mapper.Map<MetadataSource>(x.Source), x.TagId, x.Value))
            .ToList();

        await _filesClient.UnbindTagsAsync(new UnbindTagsCommand(tags));
    }

    public async Task<(IReadOnlyCollection<FileTag> Tags, IReadOnlyCollection<FileNote> Notes)> GetFileMetadata(
        Guid fileId,
        CancellationToken ct)
    {
        var info = await _filesClient.GetFileInfoAsync(fileId, ct);
        return (_mapper.Map<IReadOnlyCollection<FileTag>>(info.Tags),
            _mapper.Map<IReadOnlyCollection<FileNote>>(info.Notes));
    }

    private async Task<Tag> GetOrCreateTag(string name, string typeName, bool hasValue)
    {
        var rateTags = await _tagService.SearchTags(name, 1);
        var rateTag = rateTags.FirstOrDefault();

        var rateTagFound = rateTag != null 
                           && rateTag.Title == name 
                           && rateTag.HasValue == hasValue 
                           && rateTag.Type.Title == typeName;
        
        if (rateTagFound)
            return rateTag!;

        var types = await _tagService.GеtTypes();
        var localType = types.First(x => x.Title == typeName);
        await _tagService.CreateTag(localType.Id, name, hasValue, Array.Empty<string>());

        rateTags = await _tagService.SearchTags(name, 1);
        rateTag = rateTags.First(x => x.Title == name && x.HasValue == hasValue && x.Type.Title == typeName);

        return rateTag;
    }
}
