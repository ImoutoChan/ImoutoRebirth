using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal class TagService : ITagService
{
    private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
    private readonly IMapper _mapper;

    public TagService(IImoutoRebirthLilinWebApiClient lilinClient, IMapper mapper)
    {
        _lilinClient = lilinClient;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<TagType>> GеtTypes()
    {
        var types = await _lilinClient.TagTypes.GetAllAsync();
        return _mapper.Map<IReadOnlyCollection<TagType>>(types);
    }

    public Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms)
    {
        return _lilinClient.Tags.CreateAsync(new TagCreateRequest(typeId, name, hasValue, synonyms.ToList()));
    }

    public async Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count)
    {
        var results = await _lilinClient.Tags.SearchAsync(new TagsSearchRequest(name, count));
        return _mapper.Map<IReadOnlyCollection<Tag>>(results);
    }
}