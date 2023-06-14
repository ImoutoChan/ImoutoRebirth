using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Queries;

public record FileInfoQuery(Guid FileId) : IQuery<FileInfo>;

internal class FileInfoQueryHandler : IQueryHandler<FileInfoQuery, FileInfo>
{
    private readonly IFileInfoRepository _fileInfoRepository;

    public FileInfoQueryHandler(IFileInfoRepository fileInfoRepository) => _fileInfoRepository = fileInfoRepository;

    public Task<FileInfo> Handle(FileInfoQuery request, CancellationToken ct) 
        => _fileInfoRepository.Get(request.FileId, ct);
}
