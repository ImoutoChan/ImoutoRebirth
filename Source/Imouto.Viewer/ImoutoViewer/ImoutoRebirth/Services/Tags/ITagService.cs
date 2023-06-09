﻿using ImoutoViewer.ImoutoRebirth.Services.Tags.Model;

namespace ImoutoViewer.ImoutoRebirth.Services.Tags;

internal interface ITagService
{
    Task<IReadOnlyCollection<TagType>> GеtTypes();

    Task CreateTag(Guid typeId, string name, bool hasValue, IReadOnlyCollection<string> synonyms);

    Task<IReadOnlyCollection<Tag>> SearchTags(string name, int count);
}