using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.ApplicationServices;
using ImoutoRebirth.Lilin.Services.Extensions;
using MediatR;
using MetadataSource = ImoutoRebirth.Lilin.Core.Models.MetadataSource;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

public class SaveMetadataCommandHandler : ICommandHandler<SaveMetadataCommand>
{
    private readonly ITagTypeRepository _tagTypeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IFileInfoService _fileInfoService;

    public SaveMetadataCommandHandler(
        ITagTypeRepository tagTypeRepository,
        ITagRepository tagRepository,
        IEventStorage eventStorage,
        IFileInfoService fileInfoService)
    {
        _tagTypeRepository = tagTypeRepository;
        _tagRepository = tagRepository;
        _eventStorage = eventStorage;
        _fileInfoService = fileInfoService;
    }

    public async Task<Unit> Handle(SaveMetadataCommand request, CancellationToken cancellationToken)
    {
        var source = request.MqCommand.MetadataSource.Convert();
        var fileId = request.MqCommand.FileId;

        var file = await _fileInfoService.LoadFileAggregate(fileId);

        var newTags = await LoadTags(fileId, source, request.MqCommand.FileTags).ToReadOnlyListAsync();
        var newNotes = LoadNotes(fileId, source, request.MqCommand.FileNotes).ToList();
                
        var metadataUpdateData = new MetadataUpdateData(fileId, newTags, newNotes, source);

        var domainResult = file.UpdateMetadata(metadataUpdateData);
        _eventStorage.AddRange(domainResult.EventsCollection);
            
        await _fileInfoService.PersistFileAggregate(file);

        return Unit.Value;
    }

    private static IEnumerable<FileNote> LoadNotes(
        Guid fileId, 
        MetadataSource source, 
        IEnumerable<IFileNote> mqCommandFileNotes)
    {
        var commandFileNotes = mqCommandFileNotes as IFileNote[] 
                               ?? mqCommandFileNotes?.ToArray() 
                               ?? Array.Empty<IFileNote>();
        if (!commandFileNotes.Any())
            yield break;

        foreach (var fileNote in commandFileNotes)
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
        var commandFileTags = mqCommandFileTags as IFileTag[] 
                              ?? mqCommandFileTags?.ToArray() 
                              ?? Array.Empty<IFileTag>();
        if (!commandFileTags.Any())
            yield break;

        foreach (var fileTag in commandFileTags)
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
}