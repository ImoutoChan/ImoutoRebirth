using System;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Services
{
    public class MetadataUpdateService : IMetadataUpdateService
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MetadataUpdateService(
            IFileTagRepository fileTagRepository, 
            IFileNoteRepository fileNoteRepository, 
            IUnitOfWork unitOfWork)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ApplyMetadata(MetadataUpdate update)
        {
            if (update.MetadataSource != MetadataSource.Manual)
            {
                await ClearOldMetadata(update.FileId, update.MetadataSource);
            }

            foreach (var tag in update.Tags)
            {
                await _fileTagRepository.Add(tag);
            }

            foreach (var note in update.Notes)
            {
                await _fileNoteRepository.Add(note);
            }

            await _unitOfWork.SaveEntitiesAsync();
        }

        private async Task ClearOldMetadata(Guid fileId, MetadataSource source)
        {
            await _fileTagRepository.ClearForSource(fileId, source);
            await _fileNoteRepository.ClearForSource(fileId, source);
        }
    }
}
