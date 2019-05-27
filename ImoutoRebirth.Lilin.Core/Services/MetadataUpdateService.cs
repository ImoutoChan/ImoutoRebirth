using System;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Core.Events;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Core.Services
{
    public class MetadataUpdateService : IMetadataUpdateService
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;
        private readonly IEventStorage _eventStorage;

        public MetadataUpdateService(
            IFileTagRepository fileTagRepository, 
            IFileNoteRepository fileNoteRepository,
            IEventStorage eventStorage)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
            _eventStorage = eventStorage;
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

            _eventStorage.Add(new MetadataUpdated(update.FileId, update.MetadataSource));
        }

        private async Task ClearOldMetadata(Guid fileId, MetadataSource source)
        {
            await _fileTagRepository.ClearForSource(fileId, source);
            await _fileNoteRepository.ClearForSource(fileId, source);
        }
    }
}
