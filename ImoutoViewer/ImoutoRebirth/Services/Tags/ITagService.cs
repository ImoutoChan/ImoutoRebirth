using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags
{
    internal interface ITagService
    {
        Task<IReadOnlyCollection<TagType>> GеtTypes();

        Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms);

        Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count);
    }
}