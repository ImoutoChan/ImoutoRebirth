using System.Data;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.Core.TagAggregate;
using ImoutoRebirth.Lilin.Core.TagTypeAggregate;
using MetadataSource = ImoutoRebirth.Lilin.Core.FileInfoAggregate.MetadataSource;

namespace ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;

[CommandQuery(IsolationLevel.Serializable)]
public record ActualizeFileInfoForSourceCommand(
    Guid FileId,
    MetadataSource MetadataSource,
    IReadOnlyCollection<ActualizeTag> Tags,
    IReadOnlyCollection<ActualizeNote> Notes) : ICommand;

public record ActualizeTag(
    string Type,
    string Name,
    string? Value,
    IReadOnlyCollection<string>? Synonyms);

public record ActualizeNote(
    int? SourceId,
    string Label,
    int PositionFromLeft,
    int PositionFromTop,
    int Width,
    int Height);

internal class ActualizeFileInfoForSourceCommandHandler : ICommandHandler<ActualizeFileInfoForSourceCommand>
{
    private readonly ITagTypeRepository _tagTypeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IEventStorage _eventStorage;
    private readonly IFileInfoRepository _fileInfoRepository;

    public ActualizeFileInfoForSourceCommandHandler(
        ITagTypeRepository tagTypeRepository,
        ITagRepository tagRepository,
        IEventStorage eventStorage,
        IFileInfoRepository fileInfoRepository)
    {
        _tagTypeRepository = tagTypeRepository;
        _tagRepository = tagRepository;
        _eventStorage = eventStorage;
        _fileInfoRepository = fileInfoRepository;
    }

    public async Task Handle(ActualizeFileInfoForSourceCommand forSourceCommand, CancellationToken ct)
    {
        var (fileId, source, tags, notes) = forSourceCommand;

        var fileInfo = await _fileInfoRepository.Get(fileId, ct);

        var newFileTags = await PrepareTags(fileId, source, tags);
        var newFileNotes = PrepareNotes(fileId, source, notes).ToList();
                
        var result = fileInfo.UpdateMetadata(source, newFileTags, newFileNotes);
        
        _eventStorage.AddRange(result);
        await _fileInfoRepository.Save(fileInfo);
    }

    private static IEnumerable<FileNote> PrepareNotes(
        Guid fileId, 
        MetadataSource source, 
        IReadOnlyCollection<ActualizeNote> notes)
    {
        if (notes.None())
            yield break;

        foreach (var fileNote in notes)
        {
            yield return FileNote.Create(
                fileId,
                fileNote.Label,
                fileNote.PositionFromLeft,
                fileNote.PositionFromTop,
                fileNote.Width,
                fileNote.Height, 
                source, 
                fileNote.SourceId);
        }
    }

    private async Task<IReadOnlyCollection<FileTag>> PrepareTags(
        Guid fileId, 
        MetadataSource source, 
        IReadOnlyCollection<ActualizeTag> tags)
    {
        if (tags.None())
            return Array.Empty<FileTag>();

        var typesByName = await GetOrCreateTagTypes(tags);

        return await GetOrCreateTags(tags, typesByName, fileId, source).ToReadOnlyListAsync();
    }

    private async Task<IReadOnlyDictionary<string, TagType>> GetOrCreateTagTypes(
        IReadOnlyCollection<ActualizeTag> tags)
    {
        var typeNames = tags
            .Select(x => x.Type)
            .Distinct();

        var types = new List<TagType>();
        foreach (var typeName in typeNames)
        {
            var type = await _tagTypeRepository.Get(typeName);
            types.Add(type);
        }
        
        return types.ToDictionary(x => x.Name);
    }

    private async IAsyncEnumerable<FileTag> GetOrCreateTags(
        IReadOnlyCollection<ActualizeTag> tags, 
        IReadOnlyDictionary<string, TagType> typesByName, 
        Guid fileId,
        MetadataSource source)
    {
        foreach (var fileTag in tags)
        {
            var type = typesByName[fileTag.Type];

            var tag = await GetOrCreateTag(fileTag, type);

            yield return new FileTag(fileId, tag.Id, fileTag.Value, source);
        }
    }

    private async Task<Tag> GetOrCreateTag(ActualizeTag fileTag, TagType type)
    {
        var hasValue = !string.IsNullOrWhiteSpace(fileTag.Value);

        var tag = await _tagRepository.Get(fileTag.Name, type.Id, default);

        if (tag == null)
        {
            var newTag = Tag.Create(type, fileTag.Name, hasValue, fileTag.Synonyms);
            await _tagRepository.Create(newTag);

            return newTag;
        }
        else
        {
            tag.UpdateHasValue(hasValue);
            tag.UpdateSynonyms(fileTag.Synonyms ?? Array.Empty<string>());

            await _tagRepository.Update(tag);

            return tag;
        }
    }
}
