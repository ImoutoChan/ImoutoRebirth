using FluentAssertions;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Core.Tests;

public class FileInfoPackTests
{
    [Fact]
    public void ShouldAddFileTag_WhenHaveSameTag_WithDifferentValue_AndHaveAddNewFileTagStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string currentValue = "Value";
        const string newValue = "NewValue";

        var existsFileTag = new FileTag(fileId, tag, currentValue, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag, newValue, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        var pack = new FileInfoPack(new []{existsFileInfo});
            
        // act
        pack.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.AddNewFileTag);

        // assert
        pack.Files.First().Tags.Should().HaveCount(2);
    }

    [Fact]
    public void ShouldIgnoreFileTag_WhenHaveSameTag_WithSameValue_AndHaveAddNewFileTagStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";

        var existsFileTag = new FileTag(fileId, tag, value, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        var pack = new FileInfoPack(new []{existsFileInfo});
            
        // act
        pack.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.AddNewFileTag);

        // assert
        pack.Files.First().Tags.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldAddFileTag_WhenHaveSameTag_WithDifferentValue_AndHaveReplaceExistingValueStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string currentValue = "Value";
        const string newValue = "NewValue";

        var existsFileTag = new FileTag(fileId, tag, currentValue, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag, newValue, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        var pack = new FileInfoPack(new []{existsFileInfo});
            
        // act
        pack.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        pack.Files.First().Tags.Should().HaveCount(1);
        pack.Files.First().Tags.First().Value.Should().Be(newValue);
    }

    [Fact]
    public void ShouldIgnoreFileTag_WhenHaveSameTag_WithSameValue_AndHaveReplaceExistingValueStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";

        var existsFileTag = new FileTag(fileId, tag, value, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        var pack = new FileInfoPack(new []{existsFileInfo});
            
        // act
        pack.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        pack.Files.First().Tags.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldAddFileTag_WhenDoesNotHaveSameTag()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";
        var newFileTag = new FileTag(fileId, tag, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(Array.Empty<FileTag>(), Array.Empty<FileNote>(), fileId);

        var pack = new FileInfoPack(new []{existsFileInfo});
            
        // act
        pack.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        pack.Files.First().Tags.Should().HaveCount(1);
    }

    private static Tag CreateTag()
    {
        var tagType = new TagType(Guid.NewGuid(), "Type", 0);
        return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), 0);
    }
}