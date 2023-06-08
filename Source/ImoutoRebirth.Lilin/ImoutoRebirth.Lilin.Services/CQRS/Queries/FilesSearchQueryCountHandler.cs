using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public record FilesSearchQueryCount(IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<int>;

internal class FilesSearchQueryCountHandler : IQueryHandler<FilesSearchQueryCount, int>
{
    private readonly IFileTagRepository _fileTagRepository;

    public FilesSearchQueryCountHandler(IFileTagRepository fileTagRepository) => _fileTagRepository = fileTagRepository;

    public Task<int> Handle(FilesSearchQueryCount request, CancellationToken ct)
        => _fileTagRepository.SearchFilesCount(request.TagSearchEntries, ct);
}
