using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class FilesFilterQueryHandler : IQueryHandler<FilesFilterQuery, Guid[]>
{
    private readonly IFileTagRepository _fileTagRepository;

    public FilesFilterQueryHandler(IFileTagRepository fileTagRepository)
    {
        _fileTagRepository = fileTagRepository;
    }

    public Task<Guid[]> Handle(FilesFilterQuery request, CancellationToken ct) 
        => _fileTagRepository.FilterFiles(request.TagSearchEntries, request.FileIds, ct);
}
