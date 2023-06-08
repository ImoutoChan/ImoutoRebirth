using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries;

public record FilesFilterQuery(
    IReadOnlyCollection<Guid> FileIds,
    IReadOnlyCollection<TagSearchEntry> TagSearchEntries) : IQuery<Guid[]>;

internal class FilesFilterQueryHandler : IQueryHandler<FilesFilterQuery, Guid[]>
{
    private readonly IFileTagRepository _fileTagRepository;

    public FilesFilterQueryHandler(IFileTagRepository fileTagRepository) => _fileTagRepository = fileTagRepository;

    public Task<Guid[]> Handle(FilesFilterQuery request, CancellationToken ct) 
        => _fileTagRepository.FilterFiles(request.TagSearchEntries, request.FileIds, ct);
}
