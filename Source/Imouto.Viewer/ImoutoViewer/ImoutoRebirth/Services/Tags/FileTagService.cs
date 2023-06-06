﻿using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class FileTagService : IFileTagService
{
    private readonly ITagService _tagService;
    private readonly IMapper _mapper;
    private readonly FilesClient _filesClient;

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
                    new FileTagRequest(fileId, FileTagRequestSource.Manual, rateTag.Id, rate.Rating.ToString())
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
                    new[] {new FileTagRequest(fileId, FileTagRequestSource.Manual, favTag.Id, null)},
                    BindTagsRequestSameTagHandleStrategy.ReplaceExistingValue));
        }
        else
        {
            await _filesClient.UnbindTagFromFileAsync(
                new UnbindTagRequest(new FileTagRequest(fileId, FileTagRequestSource.Manual, favTag.Id, null)));
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
            new UnbindTagRequest(new FileTagRequest(fileId, metadataSource, tagId, null)));
    }

    public async Task<IReadOnlyCollection<FileTag>> GetFileTags(Guid fileId)
    {
        var info = await _filesClient.GetFileInfoAsync(fileId);

        return _mapper.Map<IReadOnlyCollection<FileTag>>(info.Tags);
    }

    public async Task<IReadOnlyCollection<FileNote>> GetFileNotes(Guid fileId)
    {
        var info = await _filesClient.GetFileInfoAsync(fileId);

        return _mapper.Map<IReadOnlyCollection<FileNote>>(info.Notes);
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