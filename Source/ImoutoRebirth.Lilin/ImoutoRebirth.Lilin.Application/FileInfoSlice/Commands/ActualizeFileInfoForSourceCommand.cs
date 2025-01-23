using System.Data;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;
using MetadataSource = ImoutoRebirth.Lilin.Domain.FileInfoAggregate.MetadataSource;

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
    IReadOnlyCollection<string>? Synonyms,
    TagOptions Options);

public record ActualizeNote(
    string? SourceId,
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
            return [];

        var typesByName = await GetOrCreateTagTypes(tags);
        return await GetOrCreateTags(tags, typesByName, fileId, source);
    }

    private async Task<IReadOnlyDictionary<string, TagType>> GetOrCreateTagTypes(
        IReadOnlyCollection<ActualizeTag> tags)
    {
        var typeNames = tags.Select(x => x.Type).Distinct().ToList();
        var types = await _tagTypeRepository.GetOrCreateBatch(typeNames);
        return types.ToDictionary(x => x.Name);
    }

    private async Task<IReadOnlyCollection<FileTag>> GetOrCreateTags(
        IReadOnlyCollection<ActualizeTag> tags, 
        IReadOnlyDictionary<string, TagType> typesByName, 
        Guid fileId,
        MetadataSource source)
    {
        var tagsToProcess = tags
            .Select(x => new TagToProcess(x, typesByName[x.Type], !string.IsNullOrWhiteSpace(x.Value)))
            .ToList();
        
        var foundTags = await SearchForTags(tagsToProcess);
        await UpdateFoundTagsProperties(tagsToProcess, foundTags);
        
        var toCreate = tagsToProcess
            .Where(x => !foundTags.Any(y => y.Name == x.Tag.Name && y.Type.Id == x.Type.Id))
            .Select(x => Tag.Create(x.Type, x.Tag.Name, x.HasValue, x.Tag.Synonyms, x.Tag.Options))
            .ToList();

        await _tagRepository.CreateBatch(toCreate);

        return tagsToProcess
            .Select(x =>
            {
                var tag = foundTags.FirstOrDefault(y => y.Name == x.Tag.Name && y.Type.Id == x.Type.Id)
                       ?? toCreate.First(y => y.Name == x.Tag.Name && y.Type.Id == x.Type.Id);
                
                return new FileTag(fileId, tag.Id, x.Tag.Value, source);
            })
            .ToList();
    }

    private async Task<IReadOnlyCollection<Tag>> SearchForTags(
        IReadOnlyCollection<TagToProcess> tagsToProcess)
    {
        var toSearch = tagsToProcess.Select(x => new TagIdentifier(x.Tag.Name, x.Type.Id)).ToList();
        return await _tagRepository.GetBatch(toSearch);
    }

    private async Task UpdateFoundTagsProperties(
        IReadOnlyCollection<TagToProcess> tagsToProcess, 
        IReadOnlyCollection<Tag> foundTags)
    {
        foreach (var foundTag in foundTags)
        {
            var sourceTag = tagsToProcess.First(x => x.Tag.Name == foundTag.Name && x.Type.Id == foundTag.Type.Id);

            var areSynonymsTheSame = sourceTag.Tag.Synonyms.SafeNone()
                                     || sourceTag.Tag.Synonyms!.All(x => foundTag.Synonyms.Contains(x));

            var arePropertiesTheSame
                = foundTag.HasValue == sourceTag.HasValue
                  && areSynonymsTheSame
                  && foundTag.Options == sourceTag.Tag.Options;
            
            if (arePropertiesTheSame)
                continue;

            foundTag.UpdateHasValue(sourceTag.HasValue);
            foundTag.UpdateSynonyms(sourceTag.Tag.Synonyms ?? []);
            foundTag.UpdateOptions(sourceTag.Tag.Options);

            await _tagRepository.Update(foundTag);
        }
    }

    private record TagToProcess(ActualizeTag Tag, TagType Type, bool HasValue);
}

