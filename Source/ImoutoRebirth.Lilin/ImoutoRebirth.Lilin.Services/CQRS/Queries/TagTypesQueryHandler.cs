using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public record TagTypesQuery : IQuery<IReadOnlyCollection<TagType>>;
    
internal class TagTypesQueryHandler : IQueryHandler<TagTypesQuery, IReadOnlyCollection<TagType>>
{
    private readonly ITagTypeRepository _tagTypeRepository;

    public TagTypesQueryHandler(ITagTypeRepository tagTypeRepository) 
        => _tagTypeRepository = tagTypeRepository;

    public async Task<IReadOnlyCollection<TagType>> Handle(TagTypesQuery _, CancellationToken ct) 
        => await _tagTypeRepository.GetAll(ct);
}
