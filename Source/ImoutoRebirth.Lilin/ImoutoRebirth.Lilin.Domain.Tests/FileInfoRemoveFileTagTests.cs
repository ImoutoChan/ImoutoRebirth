using FluentAssertions;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Domain.Tests;

public class FileInfoRemoveFileTagTests
{
    [Fact]
    public void ShouldRemoveFileTagWithSameSourceAndValue()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Danbooru;
        var source2 = MetadataSource.Yandere;
        var source1Tag1 = Guid.NewGuid();

        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, "value1", source1),
                new FileTag(fileId, source1Tag1, "value3", source1),
                new FileTag(fileId, source1Tag1, "value1", source2),
                new FileTag(fileId, source1Tag1, "value2", source2),
                new FileTag(fileId, source1Tag1, "value3", source2),
                new FileTag(fileId, source1Tag1, "value2", source1),
            },
            Array.Empty<FileNote>(),
            fileId);
        
        // act
        fileInfo.RemoveFileTag(source1Tag1, source1, "value2");
        
        // assert
        fileInfo.Tags.Should().HaveCount(5);
        fileInfo.Tags.Where(x => x.Source == source1 && x.Value == "value2").Should().BeEmpty();
    } 
    [Fact]
    public void ShouldNotRemoveWithDifferentValueOrSource()
    {
        // arrange
        var fileId = Guid.NewGuid();

        var source1 = MetadataSource.Danbooru;
        var source2 = MetadataSource.Yandere;
        var source1Tag1 = Guid.NewGuid();

        var fileInfo = new FileInfo(
            new[]
            {
                new FileTag(fileId, source1Tag1, "value1", source1),
                new FileTag(fileId, source1Tag1, "value3", source1),
                new FileTag(fileId, source1Tag1, "value1", source2),
                new FileTag(fileId, source1Tag1, "value2", source2),
                new FileTag(fileId, source1Tag1, "value3", source2),
            },
            Array.Empty<FileNote>(),
            fileId);
        
        // act
        fileInfo.RemoveFileTag(source1Tag1, source1, "value2");
        
        // assert
        fileInfo.Tags.Should().HaveCount(5);
    } 
}
