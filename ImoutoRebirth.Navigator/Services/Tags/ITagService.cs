using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    interface ITagService
    {
        Task<IReadOnlyCollection<TagType>> GеtTypes();

        Task CreateTag(TagType type, string name, bool hasValue, IReadOnlyCollection<string> synonyms);

        Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count);
    }
}