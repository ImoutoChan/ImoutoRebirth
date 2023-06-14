using FluentAssertions;
using ImoutoRebirth.Lilin.Core.FileInfoAggregate;
using ImoutoRebirth.Lilin.Core.TagAggregate;
using ImoutoRebirth.Lilin.Core.TagTypeAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Core.Tests;

public class FileInfoUpdateTagsTests
{
    [Fact]
    public void ShouldAddFileTag_WhenHaveSameTag_WithDifferentValue_AndHaveAddNewFileTagStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string currentValue = "Value";
        const string newValue = "NewValue";

        var existsFileTag = new FileTag(fileId, tag.Id, currentValue, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag.Id, newValue, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        // act
        existsFileInfo.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.AddNewFileTag);

        // assert
        existsFileInfo.Tags.Should().HaveCount(2);
    }

    [Fact]
    public void ShouldIgnoreFileTag_WhenHaveSameTag_WithSameValue_AndHaveAddNewFileTagStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";

        var existsFileTag = new FileTag(fileId, tag.Id, value, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag.Id, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);
            
        // act
        existsFileInfo.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.AddNewFileTag);

        // assert
        existsFileInfo.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldAddFileTag_WhenHaveSameTag_WithDifferentValue_AndHaveReplaceExistingValueStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string currentValue = "Value";
        const string newValue = "NewValue";

        var existsFileTag = new FileTag(fileId, tag.Id, currentValue, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag.Id, newValue, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        // act
        existsFileInfo.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        existsFileInfo.Tags.Should().HaveCount(1);
        existsFileInfo.Tags.First().Value.Should().Be(newValue);
    }

    [Fact]
    public void ShouldIgnoreFileTag_WhenHaveSameTag_WithSameValue_AndHaveReplaceExistingValueStrategy()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";

        var existsFileTag = new FileTag(fileId, tag.Id, value, MetadataSource.Manual);
        var newFileTag = new FileTag(fileId, tag.Id, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(new [] {existsFileTag}, Array.Empty<FileNote>(), fileId);

        // act
        existsFileInfo.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        existsFileInfo.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void ShouldAddFileTag_WhenDoesNotHaveSameTag()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string value = "Value";
        var newFileTag = new FileTag(fileId, tag.Id, value, MetadataSource.Manual);

        var existsFileInfo = new FileInfo(Array.Empty<FileTag>(), Array.Empty<FileNote>(), fileId);

        // act
        existsFileInfo.UpdateTags(new []{newFileTag}, SameTagHandleStrategy.ReplaceExistingValue);

        // assert
        existsFileInfo.Tags.Should().HaveCount(1);
    }

    private static Tag CreateTag()
    {
        var tagType = new TagType(Guid.NewGuid(), "Type", 0);
        return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), 0);
    }
}
