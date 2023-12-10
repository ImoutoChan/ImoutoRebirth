using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.TagAggregate;

namespace ImoutoRebirth.Lilin.Application.TagSlice;

[CommandQuery(IsolationLevel.Serializable)]
public record CreateTagCommand(
    Guid TypeId,
    string Name,
    bool HasValue,
    IReadOnlyCollection<string>? Synonyms,
    TagOptions Options)
    : ICommand<Tag>;

internal class CreateTagCommandHandler : ICommandHandler<CreateTagCommand, Tag>
{
    private readonly ITagRepository _tagRepository;
    private readonly ITagTypeRepository _tagTypeRepository;

    public CreateTagCommandHandler(ITagRepository tagRepository, ITagTypeRepository tagTypeRepository)
    {
        _tagRepository = tagRepository;
        _tagTypeRepository = tagTypeRepository;
    }

    public async Task<Tag> Handle(CreateTagCommand command, CancellationToken ct)
    {
        var (typeId, name, hasValue, synonyms, options) = command;

        var tag = await _tagRepository.Get(name, typeId, ct);

        if (tag != null)
        {
            tag.UpdateHasValue(hasValue);
            tag.UpdateOptions(options);

            if (synonyms != null)
                tag.UpdateSynonyms(synonyms);

            await _tagRepository.Update(tag);
        }
        else
        {
            var tagType = await _tagTypeRepository.Get(typeId, ct).GetAggregateOrThrow(typeId);

            tag = Tag.Create(tagType, name, hasValue, synonyms, options);

            await _tagRepository.Create(tag);
        }

        return await _tagRepository.Get(name, typeId, ct).GetAggregateOrThrow();
    }
}
