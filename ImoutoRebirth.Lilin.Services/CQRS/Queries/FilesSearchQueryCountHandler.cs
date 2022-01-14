using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public class FilesSearchQueryCountHandler : IQueryHandler<FilesSearchQueryCount, int>
{
    private readonly IFileTagRepository _fileTagRepository;

    public FilesSearchQueryCountHandler(IFileTagRepository fileTagRepository)
    {
        _fileTagRepository = fileTagRepository;
    }

    public Task<int> Handle(FilesSearchQueryCount request, CancellationToken cancellationToken)
        => _fileTagRepository.SearchFilesCount(request.TagSearchEntries);
}