using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class RelativesBatchQueryHandler : IQueryHandler<RelativesBatchQuery, IReadOnlyCollection<RelativeShortInfo>>
{
    private readonly IFileTagRepository _fileTagRepository;

    public RelativesBatchQueryHandler(IFileTagRepository fileTagRepository)
    {
        _fileTagRepository = fileTagRepository;
    }

    public async Task<IReadOnlyCollection<RelativeShortInfo>> Handle(
        RelativesBatchQuery request,
        CancellationToken ct)
    {
        var found = await _fileTagRepository.SearchHashesInTags(request.Md5, ct);
        return found.Select(x => new RelativeShortInfo(x.x, x.Item2)).ToList();
    }
}
