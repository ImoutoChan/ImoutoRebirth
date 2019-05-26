using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQueryCountHandler : IQueryHandler<FilesSearchQueryCount, uint>
    {
        private readonly IFileTagRepository _fileTagRepository;

        public FilesSearchQueryCountHandler(IFileTagRepository fileTagRepository)
        {
            _fileTagRepository = fileTagRepository;
        }

        public Task<uint> Handle(FilesSearchQueryCount request, CancellationToken cancellationToken)
            => _fileTagRepository.SearchFilesCount(request.TagSearchEntries);
    }
}