using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using Tag = ImoutoRebirth.Navigator.Services.Tags.Model.Tag;
using TagType = ImoutoRebirth.Navigator.Services.Tags.Model.TagType;

namespace ImoutoRebirth.Navigator.Services.Tags;

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

    public Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms, bool isCounter)
    {
        var options = isCounter ? TagOptions.Counter : TagOptions.None;
        return _tagsClient.CreateTagAsync(new CreateTagCommand(hasValue, name, options, synonyms.ToList(), typeId));
    }

    public async Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count, CancellationToken ct)
    {
        var results = await _tagsClient.SearchTagsAsync(new TagsSearchQuery(count, name), ct);
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }

    public async Task<IReadOnlyCollection<string>> SearchTagValues(Guid tagId, string? value, int count, CancellationToken ct)
    {
        return await _tagsClient.SearchTagValuesAsync(new(count, value, tagId), ct);
    }

    public async Task<IReadOnlyCollection<Tag>> GetPopularUserTags(int count)
    {
        var results = await _tagsClient.GetPopularTagsAsync(count);
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }

    public async Task<IReadOnlyCollection<Tag>> GetPopularUserCharacterTags(int count)
    {
        var results = await _tagsClient.GetPopularCharactersTagsAsync(count);
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }

    public async Task MergeTags(Guid tagToCleanId, Guid tagToEnrichId)
    {
        await _tagsClient.MergeTagsAsync(new MergeTagsCommand(tagToCleanId, tagToEnrichId));
    }

    public async Task DeleteTag(Guid tagId)
    {
        await _tagsClient.DeleteTagAsync(tagId);
    }
}
