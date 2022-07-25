using AutoMapper;
using ImoutoRebirth.LilinService.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class TagService : ITagService
{
    private readonly TagsClient _tagsClient;
    private readonly TagTypesClient _tagTypesClient;
    private readonly IMapper _mapper;

    public TagService(IMapper mapper, TagsClient tagsClient, TagTypesClient tagTypesClient)
    {
        _mapper = mapper;
        _tagsClient = tagsClient;
        _tagTypesClient = tagTypesClient;
    }

    public async Task<IReadOnlyCollection<TagType>> GеtTypes()
    {
        
        var types = await _tagTypesClient.GetAllAsync();
        return _mapper.Map<IReadOnlyCollection<TagType>>(types);
    }

    public Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms)
    {
        return _tagsClient.CreateAsync(new TagCreateRequest(hasValue, name, synonyms.ToList(), typeId));
    }

    public async Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count)
    {
        var results = await _tagsClient.SearchAsync(new TagsSearchRequest(count, name));
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }
}
