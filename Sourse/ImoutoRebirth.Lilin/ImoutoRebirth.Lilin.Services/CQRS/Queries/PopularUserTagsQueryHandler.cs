using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class PopularUserTagsQueryHandler : IQueryHandler<PopularUserTagsQuery, IReadOnlyCollection<Tag>>
{
    private readonly IFileTagRepository _fileTagRepository;

    public PopularUserTagsQueryHandler(IFileTagRepository fileTagRepository) => _fileTagRepository = fileTagRepository;

    public Task<IReadOnlyCollection<Tag>> Handle(PopularUserTagsQuery request, CancellationToken ct) 
        => _fileTagRepository.GetPopularUserTagIds(request.Limit, ct);
}
