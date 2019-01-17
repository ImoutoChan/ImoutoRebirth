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

            foreach (var tag in metadata.Tags)
            {
                await _fileTagRepository.Add(tag);
            }

            foreach (var note in metadata.Notes)
            {
                await _fileNoteRepository.Add(note);
            }

            await _fileTagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            await _fileNoteRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}