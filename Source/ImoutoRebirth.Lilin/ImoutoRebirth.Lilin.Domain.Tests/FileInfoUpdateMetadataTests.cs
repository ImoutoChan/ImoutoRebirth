using FluentAssertions;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Domain.Tests;

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

    [Fact]
    public void ShouldNotTouchOtherSources1()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Danbooru;
        var source2 = MetadataSource.Yandere;
        var source1Tag1 = Guid.NewGuid();
        var source1Tag2 = Guid.NewGuid();
        var source1Tag3 = Guid.NewGuid();
        var source1Tag4 = Guid.NewGuid();
        var source1Tag5 = Guid.NewGuid();
        var source2Tag1 = Guid.NewGuid();
        var source2Tag2 = Guid.NewGuid();
        var source2Tag3 = Guid.NewGuid();

        var source1Tags = new[] {source1Tag1, source1Tag2, source1Tag3, source1Tag4, source1Tag5};
        var source1Notes = new[] { "note1", "note2", "note3", "note4", "note5" };
        
        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, null, source1),
                new FileTag(fileId, source1Tag2, null, source1),
                new FileTag(fileId, source1Tag3, null, source1),
                new FileTag(fileId, source1Tag4, null, source1),
                new FileTag(fileId, source1Tag5, null, source1),
                new FileTag(fileId, source2Tag1, null, source2),
                new FileTag(fileId, source2Tag2, null, source2),
                new FileTag(fileId, source2Tag3, null, source2),
            },
            new[]
            {
                CreateSomeNote(fileId, source1, "note1"),
                CreateSomeNote(fileId, source1, "note2"),
                CreateSomeNote(fileId, source1, "note3"),
                CreateSomeNote(fileId, source1, "note4"),
                CreateSomeNote(fileId, source1, "note5"),
                CreateSomeNote(fileId, source2, "note6"),
                CreateSomeNote(fileId, source2, "note7"),
                CreateSomeNote(fileId, source2, "note8"),
            },
            fileId);
        
        // act
        fileInfo.UpdateMetadata(source2, Array.Empty<FileTag>(), Array.Empty<FileNote>());
        
        // assert
        fileInfo.Tags.Should().HaveCount(5);
        fileInfo.Notes.Should().HaveCount(5);
        fileInfo.Tags.Select(x => x.TagId).Should().BeEquivalentTo(source1Tags);
        fileInfo.Notes.Select(x => x.Label).Should().BeEquivalentTo(source1Notes);
    }

    [Fact]
    public void ShouldNotTouchOtherSources2()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Danbooru;
        var source2 = MetadataSource.Yandere;
        var source1Tag1 = Guid.NewGuid();
        var source1Tag2 = Guid.NewGuid();
        var source1Tag3 = Guid.NewGuid();
        var source1Tag4 = Guid.NewGuid();
        var source1Tag5 = Guid.NewGuid();
        var source2Tag1 = Guid.NewGuid();
        var source2Tag2 = Guid.NewGuid();
        var source2Tag3 = Guid.NewGuid();

        var source2Tags = new[] {source2Tag1, source2Tag2, source2Tag3};
        var source2Notes = new[] { "note6", "note7", "note8" };
        
        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, null, source1),
                new FileTag(fileId, source1Tag2, null, source1),
                new FileTag(fileId, source1Tag3, null, source1),
                new FileTag(fileId, source1Tag4, null, source1),
                new FileTag(fileId, source1Tag5, null, source1),
                new FileTag(fileId, source2Tag1, null, source2),
                new FileTag(fileId, source2Tag2, null, source2),
                new FileTag(fileId, source2Tag3, null, source2),
            },
            new[]
            {
                CreateSomeNote(fileId, source1, "note1"),
                CreateSomeNote(fileId, source1, "note2"),
                CreateSomeNote(fileId, source1, "note3"),
                CreateSomeNote(fileId, source1, "note4"),
                CreateSomeNote(fileId, source1, "note5"),
                CreateSomeNote(fileId, source2, "note6"),
                CreateSomeNote(fileId, source2, "note7"),
                CreateSomeNote(fileId, source2, "note8"),
            },
            fileId);
        
        // act
        fileInfo.UpdateMetadata(source1, Array.Empty<FileTag>(), Array.Empty<FileNote>());
        
        // assert
        fileInfo.Tags.Should().HaveCount(3);
        fileInfo.Notes.Should().HaveCount(3);
        fileInfo.Tags.Select(x => x.TagId).Should().BeEquivalentTo(source2Tags);
        fileInfo.Notes.Select(x => x.Label).Should().BeEquivalentTo(source2Notes);
    } 

    [Fact]
    public void ShouldReplaceTagsAndNotesForSource()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Danbooru;
        var source1Tag1 = Guid.NewGuid();
        var source1Tag2 = Guid.NewGuid();
        var source1Tag3 = Guid.NewGuid();
        var source1Tag4 = Guid.NewGuid();
        var source1Tag5 = Guid.NewGuid();

        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, null, source1),
                new FileTag(fileId, source1Tag2, null, source1),
                new FileTag(fileId, source1Tag3, null, source1),
            },
            new[]
            {
                CreateSomeNote(fileId, source1, "note1", 1),
                CreateSomeNote(fileId, source1, "note2", 2),
            },
            fileId);
        
        // act
        fileInfo.UpdateMetadata(source1, new []
        {
            new FileTag(fileId, source1Tag3, null, source1),
            new FileTag(fileId, source1Tag4, null, source1),
            new FileTag(fileId, source1Tag5, null, source1),
        }, new []
        {
            CreateSomeNote(fileId, source1, "note2", 2),
            CreateSomeNote(fileId, source1, "note3", 3),
            CreateSomeNote(fileId, source1, "note4", 4),
        });
        
        // assert
        fileInfo.Tags.Should().HaveCount(3);
        fileInfo.Notes.Should().HaveCount(3);
        fileInfo.Tags.Select(x => x.TagId).Should().BeEquivalentTo(new[] { source1Tag3, source1Tag4, source1Tag5 });
        fileInfo.Notes.Select(x => x.Label).Should().BeEquivalentTo("note2", "note3", "note4");
    } 

    [Fact]
    public void ShouldAddTagsAndNotesForManualSource()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Manual;
        var source1Tag1 = Guid.NewGuid();
        var source1Tag2 = Guid.NewGuid();
        var source1Tag3 = Guid.NewGuid();
        var source1Tag4 = Guid.NewGuid();
        var source1Tag5 = Guid.NewGuid();

        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, null, source1),
                new FileTag(fileId, source1Tag2, null, source1),
                new FileTag(fileId, source1Tag3, null, source1),
            },
            new[]
            {
                CreateSomeNote(fileId, source1, "note1", 1),
                CreateSomeNote(fileId, source1, "note2", 2),
            },
            fileId);
        
        // act
        fileInfo.UpdateMetadata(source1, new []
        {
            new FileTag(fileId, source1Tag3, null, source1),
            new FileTag(fileId, source1Tag4, null, source1),
            new FileTag(fileId, source1Tag5, null, source1),
        }, new []
        {
            CreateSomeNote(fileId, source1, "note2", 2),
            CreateSomeNote(fileId, source1, "note3", 3),
            CreateSomeNote(fileId, source1, "note4", 4),
        });
        
        // assert
        fileInfo.Tags.Should().HaveCount(5);
        fileInfo.Notes.Should().HaveCount(4);
        fileInfo.Tags.Select(x => x.TagId).Should().BeEquivalentTo(new[]
            { source1Tag1, source1Tag2, source1Tag3, source1Tag4, source1Tag5 });
        fileInfo.Notes.Select(x => x.Label).Should().BeEquivalentTo("note1", "note2", "note3", "note4");
    } 
    
    private static Tag CreateSomeTag()
    {
        var tagType = new TagType(Guid.NewGuid(), "Type", default);
        return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), default);
    }
        
    private static FileNote CreateSomeNote(Guid fileId, MetadataSource source, string text = "", int? sourceId = default) 
        => new(fileId, text, default, default, default, default, source, sourceId);
}
