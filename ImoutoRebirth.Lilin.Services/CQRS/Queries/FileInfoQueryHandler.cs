using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class FileInfoQueryHandler : IQueryHandler<FileInfoQuery, FileInfo>
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;

        public FileInfoQueryHandler(IFileTagRepository fileTagRepository, IFileNoteRepository fileNoteRepository)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
        }

        public async Task<FileInfo> Handle(FileInfoQuery request, CancellationToken cancellationToken)
        {
            var tags = await _fileTagRepository.GetForFile(request.FileId);
            var notes = await _fileNoteRepository.GetForFile(request.FileId);

            return new FileInfo(tags, notes);
        }
    }
}