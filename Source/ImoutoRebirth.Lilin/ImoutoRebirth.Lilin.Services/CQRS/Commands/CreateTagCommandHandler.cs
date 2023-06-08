using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.Serializable)]
public record CreateTagCommand(Guid TypeId, string Name, bool HasValue, IReadOnlyCollection<string>? Synonyms) 
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

    public async Task<Tag> Handle(CreateTagCommand request, CancellationToken ct)
    {
        var tag = await _tagRepository.Get(request.Name, request.TypeId, ct);

        if (tag != null)
        {
            tag.UpdateHasValue(request.HasValue);

            if (request.Synonyms != null)
                tag.UpdateSynonyms(request.Synonyms);

            await _tagRepository.Update(tag);
        }
        else
        {
            var tagType = await _tagTypeRepository.Get(request.TypeId, default);

            if (tagType == null)
                throw new ApplicationException($"TagType not found id: {request.TypeId}.");

            tag = Tag.CreateNew(tagType, request.Name, request.HasValue, request.Synonyms);

            await _tagRepository.Create(tag);
        }

        var resultTag = await _tagRepository.Get(request.Name, request.TypeId, default);

        if (resultTag == null)
            throw new ApplicationException($"Tag was not created.");

        return resultTag;
    }
}
