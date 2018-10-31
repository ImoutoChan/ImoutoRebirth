using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services
{
    public class MetadataPersister
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;

        public MetadataPersister(IFileTagRepository fileTagRepository, IFileNoteRepository fileNoteRepository)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
        }

        public Task UpdateMetadata(MetadataUpdate metadata)
        {
            foreach (var metadataTag in metadata.Tags)
                _fileTagRepository.Add(metadataTag);

            var addTagTasks = metadata.Tags.Select(x => _fileTagRepository.Add(x));
            var addNoteTasks = metadata.Notes.Select(x => _fileNoteRepository.Add(x));

            return Task.WhenAll(addTagTasks.Union(addNoteTasks));
        }
    }

    public class CommandHandler<TCommand>
    {
        private readonly ICommandHandler<TCommand> _commandHandler;
        private readonly IUnitOfWork _unitOfWork;

        public CommandHandler(ICommandHandler<TCommand> commandHandler, IUnitOfWork unitOfWork)
        {
            _commandHandler = commandHandler;
            _unitOfWork = unitOfWork;
        }

        public Task Execute(TCommand command)
        {
            _unitOfWork.Init();

            await _commandHandler.ExecuteHandler(command);

            _unitOfWork.Commit();
        }
    }

    public interface ICommandHandler<in TCommand>
    {
        Task ExecuteHandler(TCommand command);
    }
}
