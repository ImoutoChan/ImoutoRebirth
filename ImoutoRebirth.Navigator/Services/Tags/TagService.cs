using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Lilin.WebApi.Client.Models;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal class TagService : ITagService
    {
        private readonly IImoutoRebirthLilinWebApiClient _lilinClient;
        private readonly IMapper _mapper;

        public TagService(IImoutoRebirthLilinWebApiClient lilinClient, IMapper mapper)
        {
            _lilinClient = lilinClient;
            _mapper = mapper;
        }

        public Task<IReadOnlyCollection<TagType>> GеtTypes()
        {
            throw new System.NotImplementedException();
        }

        public Task CreateTag(TagType type, string name, bool hasValue, IReadOnlyCollection<string> synonyms)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count)
        {
            var results = await _lilinClient.Tags.SearchAsync(new TagsSearchRequest(name, count));
            return _mapper.Map<IReadOnlyCollection<Tag>>(results);
        }
    }
}