using FluentAssertions;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.Core.TagAggregate;
using ImoutoRebirth.Lilin.Core.TagTypeAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Core.Tests;

public class FileInfoUpdateMetadataTests
{
    private const string CurrentTagValue = "Value";
    private const MetadataSource CurrentSource = MetadataSource.Danbooru;
    private const MetadataSource NewSource = MetadataSource.Sankaku;
    private const string NewTagValue = "NewValue";

    private readonly Guid _fileId = Guid.NewGuid();

    [Fact]
    public void ShouldPreserveOtherFileTags_WhenUpdateMetadata_WithDifferentSource()
    {
        // arrange
        var existsFileTag = new FileTag(_fileId, CreateSomeTag().Id, CurrentTagValue, CurrentSource);
        var newFileTag = new FileTag(_fileId, CreateSomeTag().Id, NewTagValue, NewSource);

        var fileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), _fileId);
            
        // act
        fileInfo.UpdateMetadata(NewSource, new[] {newFileTag}, Array.Empty<FileNote>());

        // assert
        fileInfo.Tags.Should().Contain(existsFileTag);
    }

    [Fact]
    public void ShouldPreserveOtherFileNotes_WhenUpdateMetadata_WithDifferentSource()
    {
        // arrange
        var existingNote = CreateSomeNote(_fileId, CurrentSource);
        var newNote = CreateSomeNote(_fileId, NewSource);

        var fileInfo = new FileInfo(Array.Empty<FileTag>(), new [] {existingNote}, _fileId);
            
        // act
        fileInfo.UpdateMetadata(NewSource, Array.Empty<FileTag>(), new[] { newNote });

        // assert
        fileInfo.Notes.Should().Contain(existingNote);
    }

    [Fact]
    public void ShouldRemoveFileTags_WhenUpdateMetadata_WithSameSource()
    {
        // arrange
        var existsFileTag1 = new FileTag(_fileId, CreateSomeTag().Id, CurrentTagValue, CurrentSource);
        var existsFileTag2 = new FileTag(_fileId, CreateSomeTag().Id, CurrentTagValue, CurrentSource);
        var existsFileTag3 = new FileTag(_fileId, CreateSomeTag().Id, CurrentTagValue, CurrentSource);
        var newFileTag = new FileTag(_fileId, CreateSomeTag().Id, NewTagValue, CurrentSource);

        var fileInfo = new FileInfo(
            new[] {existsFileTag1, existsFileTag2, existsFileTag3},
            Array.Empty<FileNote>(),
            _fileId);
            
        // act
        fileInfo.UpdateMetadata(CurrentSource,
            new[] {newFileTag},
            Array.Empty<FileNote>());

        // assert
        fileInfo.Tags.Should().HaveCount(1);
        fileInfo.Tags.Should().Contain(newFileTag);
    }

    [Fact]
    public void ShouldRemoveFileNote_WhenUpdateMetadata_WithSameSource()
    {
        // arrange
        var existingNote1 = CreateSomeNote(_fileId, CurrentSource);
        var existingNote2 = CreateSomeNote(_fileId, CurrentSource);
        var existingNote3 = CreateSomeNote(_fileId, CurrentSource);
        var newNote = CreateSomeNote(_fileId, CurrentSource);

        var fileInfo = new FileInfo(
            Array.Empty<FileTag>(),
            new[] {existingNote1, existingNote2, existingNote3},
            _fileId);
            
        // act
        fileInfo.UpdateMetadata(CurrentSource, Array.Empty<FileTag>(), new[] {newNote});

        // assert
        fileInfo.Notes.Should().HaveCount(1);
        fileInfo.Notes.Should().Contain(newNote);
    }

    [Fact]
    public void ShouldReturnDomainEvent_ByDefault()
    {
        // arrange
        var newNote = CreateSomeNote(_fileId, NewSource);

        var fileInfo = new FileInfo(Array.Empty<FileTag>(), Array.Empty<FileNote>(), _fileId);
            
        // act
        var result = fileInfo.UpdateMetadata(NewSource, Array.Empty<FileTag>(), new[] { newNote });

        // assert
        result.EventsCollection.Should().HaveCount(1);
        result.EventsCollection.First().Should().BeOfType<FileInfoUpdatedDomainEvent>();

        var eventObject = result.EventsCollection.OfType<FileInfoUpdatedDomainEvent>().First();
        eventObject.FileId.Should().Be(_fileId);
        eventObject.MetadataSource.Should().Be(NewSource);
    }
    
    public void 
    
    private static Tag CreateSomeTag()
    {
        var tagType = new TagType(Guid.NewGuid(), "Type", default);
        return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), default);
    }
        
    private static FileNote CreateSomeNote(Guid fileId, MetadataSource source) =>
        new(fileId, "", default, default, default, default, source, default);
}
