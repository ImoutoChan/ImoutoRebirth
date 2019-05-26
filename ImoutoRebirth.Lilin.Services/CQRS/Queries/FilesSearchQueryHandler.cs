using System;
using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FilesSearchQueryHandler : IQueryHandler<FilesSearchQuery, Guid[]>
    {
        private readonly IFileTagRepository _fileTagRepository;

        public FilesSearchQueryHandler(IFileTagRepository fileTagRepository)
        {
            _fileTagRepository = fileTagRepository;
        }

        public Task<Guid[]> Handle(FilesSearchQuery request, CancellationToken cancellationToken) 
            => _fileTagRepository.SearchFiles(request.TagSearchEntries, request.Limit, request.Offset);
    }
}