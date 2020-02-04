using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.Extensions;
using MediatR;
using MetadataSource = ImoutoRebirth.Lilin.Core.Models.MetadataSource;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class SaveMetadataCommandHandler : ICommandHandler<SaveMetadataCommand>
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly IFileNoteRepository _fileNoteRepository;
        private readonly ITagTypeRepository _tagTypeRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IEventStorage _eventStorage;

        public SaveMetadataCommandHandler(
            IFileTagRepository fileTagRepository,
            IFileNoteRepository fileNoteRepository,
            ITagTypeRepository tagTypeRepository,
            ITagRepository tagRepository,
            IEventStorage eventStorage)
        {
            _fileTagRepository = fileTagRepository;
            _fileNoteRepository = fileNoteRepository;
            _tagTypeRepository = tagTypeRepository;
            _tagRepository = tagRepository;
            _eventStorage = eventStorage;
        }

        public async Task<Unit> Handle(SaveMetadataCommand request, CancellationToken cancellationToken)
        {
            var source = request.MqCommand.MetadataSource.Convert();
            var fileId = request.MqCommand.FileId;

            var file = await LoadFileAggregate(fileId);

            var newTags = await LoadTags(fileId, source, request.MqCommand.FileTags).ToReadOnlyListAsync();
            var newNotes = LoadNotes(fileId, source, request.MqCommand.FileNotes).ToList();
                
            var metadataUpdateData = new MetadataUpdateData(fileId, newTags, newNotes, source);

            var domainResult = file.UpdateMetadata(metadataUpdateData);
            _eventStorage.AddRange(domainResult.EventsCollection);
            
            await PersistFileAggregate(file);

            return Unit.Value;
        }

        private static IEnumerable<FileNote> LoadNotes(
            Guid fileId, 
            MetadataSource source, 
            IEnumerable<IFileNote> mqCommandFileNotes)
        {
            foreach (var fileNote in mqCommandFileNotes)
            {
                var note = Note.CreateNew(
                    fileNote.Label, 
                    fileNote.PositionFromLeft, 
                    fileNote.PositionFromTop,
                    fileNote.Width, 
                    fileNote.Height);

                yield return new FileNote(fileId, note, source, fileNote.SourceId);
            }
        }

        private async IAsyncEnumerable<FileTag> LoadTags(
            Guid fileId, 
            MetadataSource source, 
            IEnumerable<IFileTag> mqCommandFileTags)
        {
            foreach (var fileTag in mqCommandFileTags)
            {
                var type = await _tagTypeRepository.Get(fileTag.Type) 
                           ?? await _tagTypeRepository.Create(fileTag.Type);

                var tag = await GetAndUpdateTag(fileTag, type) 
                          ?? await CreateTag(fileTag, type);

                yield return new FileTag(fileId, tag, fileTag.Value, source);
            }
        }

        private async Task<Tag?> GetAndUpdateTag(IFileTag fileTag, TagType type)
        {
            var tag = await _tagRepository.Get(fileTag.Name, type.Id);

            if (tag == null)
                return null;

            tag.UpdateHasValue(!string.IsNullOrWhiteSpace(fileTag.Value));
            tag.UpdateSynonyms(fileTag.Synonyms ?? Array.Empty<string>());

            await _tagRepository.Update(tag);

            return tag;
        }

        private async Task<Tag> CreateTag(IFileTag fileTag, TagType type)
        {
            var hasValue = !string.IsNullOrWhiteSpace(fileTag.Value);

            var newTag = Tag.CreateNew(type, fileTag.Name, hasValue, fileTag.Synonyms);
            await _tagRepository.Create(newTag);

            return await _tagRepository.Get(fileTag.Name, type.Id)
                   ?? throw new ApplicationException("Tag was not created");
        }

        private async Task PersistFileAggregate(FileInfo file)
        {
            await PersistNotes(file);
            await PersistTags(file);
        }

        private async Task PersistNotes(FileInfo file)
        {
            var existingNotes = (await _fileNoteRepository.GetForFile(file.Id)).ToList();
            foreach (var newNote in file.Notes)
            {
                var existedNote = existingNotes.FirstOrDefault(x => x.IsSameIdentity(newNote));

                if (existedNote != null)
                {
                    await _fileNoteRepository.Update(existedNote.Note.Id, newNote.Note);
                    existingNotes.Remove(existedNote);
                }
                else
                {
                    await _fileNoteRepository.Add(newNote);
                }
            }

            foreach (var existingNote in existingNotes)
            {
                await _fileNoteRepository.Delete(existingNote.Note.Id);
            }
        }

        private async Task PersistTags(FileInfo file)
        {
            var existingTags = (await _fileTagRepository.GetForFile(file.Id)).ToList();
            foreach (var newTag in file.Tags)
            {
                var existedTag = existingTags.FirstOrDefault(x => x.IsSameIdentity(newTag));

                if (existedTag != null)
                {
                    await _fileTagRepository.Update(newTag);
                    existingTags.Remove(existedTag);
                }
                else
                {
                    await _fileTagRepository.Add(newTag);
                }
            }

            foreach (var existingTag in existingTags)
            {
                await _fileTagRepository.Delete(existingTag);
            }
        }

        private async Task<FileInfo> LoadFileAggregate(Guid fileId)
        {
            var tags = await _fileTagRepository.GetForFile(fileId);
            var notes = await _fileNoteRepository.GetForFile(fileId);

            return new FileInfo(tags, notes, fileId);
        }
    }
}