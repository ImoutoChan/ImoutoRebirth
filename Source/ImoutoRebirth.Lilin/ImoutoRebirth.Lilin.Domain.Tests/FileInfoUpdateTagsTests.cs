using AwesomeAssertions;
using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Lilin.Domain.FileInfoAggregate;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.Domain.TagTypeAggregate;
using Xunit;

namespace ImoutoRebirth.Lilin.Domain.Tests;

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
    public void ShouldThrow_WhenUpdatingTags_WithWrongStrategy()
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
        Assert.Throws<ArgumentOutOfRangeException>(() => existsFileInfo.UpdateTags(new []{newFileTag}, (SameTagHandleStrategy)100));
    }
    
    [Fact]
    public void ShouldThrow_WhenUpdatingTags_WithDifferentSource()
    {
        // arrange
        var tag = CreateTag();
        var fileId = Guid.NewGuid();
        const string currentValue = "Value";
        const string newValue = "NewValue";

        var fileTag1 = new FileTag(fileId, tag.Id, currentValue, MetadataSource.Yandere);
        var fileTag2 = new FileTag(fileId, tag.Id, newValue, MetadataSource.Danbooru);

        var existsFileInfo = new FileInfo(Array.Empty<FileTag>(), Array.Empty<FileNote>(), fileId);

        // act / assert
        Assert.Throws<DomainException>(() => existsFileInfo.UpdateTags([fileTag1, fileTag2], SameTagHandleStrategy.AddNewFileTag));
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
        return new Tag(Guid.NewGuid(), tagType, "Tag", true, Array.Empty<string>(), TagOptions.None, 0);
    }
}
