using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Services.CQRS.Abstract;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class SaveMetadataCommandHandler : ICommandHandler<SaveMetadataCommand>
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;

        public SaveMetadataCommandHandler(IFileTagRepository fileTagRepository, IFileNoteRepository fileNoteRepository)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
        }

        public async Task<Unit> Handle(SaveMetadataCommand request, CancellationToken cancellationToken)
        {
            var metadata = request.Update;

            var addTagTasks = metadata.Tags.Select(x => _fileTagRepository.Add(x));
            var addNoteTasks = metadata.Notes.Select(x => _fileNoteRepository.Add(x));

            await Task.WhenAll(addTagTasks.Union(addNoteTasks));

            await _fileTagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            await _fileNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}