﻿using AutoMapper;
using ImoutoRebirth.Common;
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
        var rateTag = await GetOrCreateTag("Rate", "LocalMeta", true, false);

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
        var favTag = await GetOrCreateTag("Favorite", "LocalMeta", false, false);


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

    public async Task SetWasWallpaper(Guid fileId)
    {
        var tag = await GetOrCreateTag("my wallpapers", "LocalMeta", false, false);

        await _filesClient.BindTagsAsync(
            new BindTagsCommand(
                new List<BindTag> { new(fileId, MetadataSource.Manual, tag.Id, null) },
                SameTagHandleStrategy.ReplaceExistingValue));
    }

    public async Task BindTags(
        IReadOnlyCollection<FileTag> fileTags,
        SameTagHandleStrategy strategy = SameTagHandleStrategy.AddNewFileTag)
    {
        var requests = _mapper.Map<IReadOnlyCollection<BindTag>>(fileTags);

        await _filesClient.BindTagsAsync(
            new BindTagsCommand(requests, strategy));
    }

    public async Task UnbindTags(params UnbindTagRequest[] tagsToUnbind)
    {
        var tags = tagsToUnbind
            .Select(x => new BindTag(x.FileId, _mapper.Map<MetadataSource>(x.Source), x.TagId, x.Value))
            .ToList();

        await _filesClient.UnbindTagsAsync(new UnbindTagsCommand(tags));
    }

    public async Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId)
    {
        var info = await _filesClient.GetFileInfoAsync(fileId);

        return _mapper.Map<IReadOnlyCollection<FileTag>>(info.Tags);
    }

    private async Task<Tag> GetOrCreateTag(string name, string typeName, bool hasValue, bool isCounter)
    {
        var tags = await _tagService.SearchTags(name, 1);
        var tag = tags.FirstOrDefault();

        var tagFound = tag != null 
                           && tag.Title == name 
                           && tag.HasValue == hasValue 
                           && tag.Type.Title == typeName;
        
        if (tagFound)
            return tag!;

        var types = await _tagService.GеtTypes();
        var localType = types.First(x => x.Title == typeName);
        await _tagService.CreateTag(localType.Id, name, hasValue, Array.Empty<string>(), isCounter);

        tags = await _tagService.SearchTags(name, 1);
        tag = tags.First(x => x.Title == name && x.HasValue == hasValue && x.Type.Title == typeName);

        return tag;
    }
}
