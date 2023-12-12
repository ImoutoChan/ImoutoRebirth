using ImoutoRebirth.Navigator.Services.Tags.Model;

namespace ImoutoRebirth.Navigator.Services.Tags;

internal interface ITagService
{
    Task<IReadOnlyCollection<TagType>> GеtTypes();

    Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms, bool isCounter);

    Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count);
    
    Task<IReadOnlyCollection<Tag>> GetPopularUserTags(int count);

    Task<IReadOnlyCollection<Tag>> GetPopularUserCharacterTags(int count);
}
