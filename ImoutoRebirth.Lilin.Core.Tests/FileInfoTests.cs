using FluentAssertions;
using ImoutoRebirth.Lilin.Core.Events;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Core.Tests
{
    public class FileInfoTests
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
            var existsFileTag = new FileTag(_fileId, CreateSomeTag(), CurrentTagValue, CurrentSource);
            var newFileTag = new FileTag(_fileId, CreateSomeTag(), NewTagValue, NewSource);

            var fileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), _fileId);

            var updateData = new MetadataUpdateData(
                _fileId,
                new[] {newFileTag},
                Array.Empty<FileNote>(),
                NewSource);
            
            // act
            fileInfo.UpdateMetadata(updateData);

            // assert
            fileInfo.Tags.Should().Contain(existsFileTag);
        }

        [Fact]
        public void ShouldPreserveOtherFileNotes_WhenUpdateMetadata_WithDifferentSource()
        {
            // arrange
            var existingNote = new FileNote(_fileId, CreateSomeNote(), CurrentSource, null);
            var newNote = new FileNote(_fileId, CreateSomeNote(), NewSource, null);

            var fileInfo = new FileInfo(Array.Empty<FileTag>(), new [] {existingNote}, _fileId);

            var updateData = new MetadataUpdateData(
                _fileId,
                Array.Empty<FileTag>(),
                new[] {newNote},
                NewSource);
            
            // act
            fileInfo.UpdateMetadata(updateData);

            // assert
            fileInfo.Notes.Should().Contain(existingNote);
        }

        [Fact]
        public void ShouldRemoveFileTags_WhenUpdateMetadata_WithSameSource()
        {
            // arrange
            var existsFileTag1 = new FileTag(_fileId, CreateSomeTag(), CurrentTagValue, CurrentSource);
            var existsFileTag2 = new FileTag(_fileId, CreateSomeTag(), CurrentTagValue, CurrentSource);
            var existsFileTag3 = new FileTag(_fileId, CreateSomeTag(), CurrentTagValue, CurrentSource);
            var newFileTag = new FileTag(_fileId, CreateSomeTag(), NewTagValue, CurrentSource);

            var fileInfo = new FileInfo(
                new[] {existsFileTag1, existsFileTag2, existsFileTag3},
                Array.Empty<FileNote>(),
                _fileId);

            var updateData = new MetadataUpdateData(
                _fileId,
                new[] {newFileTag},
                Array.Empty<FileNote>(),
                CurrentSource);
            
            // act
            fileInfo.UpdateMetadata(updateData);

            // assert
            fileInfo.Tags.Should().HaveCount(1);
            fileInfo.Tags.Should().Contain(newFileTag);
        }

        [Fact]
        public void ShouldRemoveFileNote_WhenUpdateMetadata_WithSameSource()
        {
            // arrange
            var existingNote1 = new FileNote(_fileId, CreateSomeNote(), CurrentSource, null);
            var existingNote2 = new FileNote(_fileId, CreateSomeNote(), CurrentSource, null);
            var existingNote3 = new FileNote(_fileId, CreateSomeNote(), CurrentSource, null);
            var newNote = new FileNote(_fileId, CreateSomeNote(), CurrentSource, null);

            var fileInfo = new FileInfo(
                Array.Empty<FileTag>(),
                new[] {existingNote1, existingNote2, existingNote3},
                _fileId);

            var updateData = new MetadataUpdateData(
                _fileId,
                Array.Empty<FileTag>(),
                new[] {newNote},
                CurrentSource);
            
            // act
            fileInfo.UpdateMetadata(updateData);

            // assert
            fileInfo.Notes.Should().HaveCount(1);
            fileInfo.Notes.Should().Contain(newNote);
        }

        [Fact]
        public void MetadataUpdateDataConstructor_ShouldThrow_WhenProvidedWithTags_WithWrongSource()
        {
            // arrange
            var newFileTag = new FileTag(_fileId, CreateSomeTag(), NewTagValue, NewSource);
            
            // act && assert
            Assert.Throws<ArgumentException>(
                () => new MetadataUpdateData(
                    _fileId, 
                    new []{newFileTag}, 
                    Array.Empty<FileNote>(), 
                    CurrentSource));
        }

        [Fact]
        public void ShouldReturnDomainEvent_ByDefault()
        {
            // arrange
            var newNote = new FileNote(_fileId, CreateSomeNote(), NewSource, null);

            var fileInfo = new FileInfo(Array.Empty<FileTag>(), Array.Empty<FileNote>(), _fileId);

            var updateData = new MetadataUpdateData(
                _fileId,
                Array.Empty<FileTag>(),
                new[] {newNote},
                NewSource);
            
            // act
            var result = fileInfo.UpdateMetadata(updateData);

            // assert
            result.EventsCollection.Should().HaveCount(1);
            result.EventsCollection.First().Should().BeOfType<MetadataUpdated>();

            var eventObject = result.EventsCollection.OfType<MetadataUpdated>().First();
            eventObject.FileId.Should().Be(_fileId);
            eventObject.MetadataSource.Should().Be(NewSource);
        }
        
        private static Tag CreateSomeTag()
        {
            var tagType = new TagType(Guid.NewGuid(), "Type", default);
            return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), default);
        }
        
        private static Note CreateSomeNote() => new Note(Guid.NewGuid(), "", default, default, default, default);
    }
}
