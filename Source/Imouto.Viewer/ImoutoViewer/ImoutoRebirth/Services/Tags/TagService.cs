﻿using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using Tag = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.Tag;
using TagType = ImoutoViewer.ImoutoRebirth.Services.Tags.Model.TagType;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal class TagService : ITagService
{
    private readonly TagsClient _tagsClient;
    private readonly IMapper _mapper;

    public TagService(IMapper mapper, TagsClient tagsClient)
    {
        _mapper = mapper;
        _tagsClient = tagsClient;
    }

    public async Task<IReadOnlyCollection<TagType>> GеtTypes()
    {
        
        var types = await _tagsClient.GetTagTypesAsync();
        return _mapper.Map<IReadOnlyCollection<TagType>>(types);
    }

    public Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms)
    {
        return _tagsClient.CreateTagAsync(new CreateTagCommand(hasValue, name, TagOptions.None, synonyms.ToList(),
            typeId));
    }

    public async Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count)
    {
        var results = await _tagsClient.SearchTagsAsync(new TagsSearchQuery(count, name));
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }

    public async Task<IReadOnlyCollection<Tag>> GetPopularUserTags(int count)
    {
        var results = await _tagsClient.GetPopularTagsAsync(count);
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }
}
