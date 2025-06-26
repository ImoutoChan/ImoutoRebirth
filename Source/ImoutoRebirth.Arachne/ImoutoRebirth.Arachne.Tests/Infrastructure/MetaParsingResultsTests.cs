using AwesomeAssertions;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Models;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure;

public class MetaParsingResultsTests
{
    [Fact]
    public void ShouldReturnAllSimpleTags()
    {
        var generatedTags = Enumerable.Range(0, 11).Select(x => $"tag{x}").ToArray();

        var obj = new MetaParsingResults(
            source: generatedTags[0],
            booruPostId: generatedTags[1],
            booruLastUpdate: generatedTags[2],
            md5: generatedTags[3],
            postedDateTime: generatedTags[4],
            postedById: generatedTags[5],
            postedByUsername: generatedTags[6],
            rating: generatedTags[7],
            ratingSafeLevel: generatedTags[10],
            parentId: generatedTags[8],
            parentMd5: generatedTags[9],
            childs: Array.Empty<string>(),
            pools: Array.Empty<string>(),
            tags: Array.Empty<MetaParsingTagResults>(),
            notes: Array.Empty<Note>(),
            null);

        var metaTags = obj.GetMetaTags().ToList();

        var stringTags = metaTags.Select(y => y.Value).ToArray();

        foreach (var x in generatedTags)
        {
            stringTags.Should().Contain(x);
        }
    }

    [Fact]
    public void ShouldReturnPools()
    {
        var generatedTags = Enumerable.Range(0, 10).Select(x => $"tag{x}").ToArray();

        var obj = new MetaParsingResults(
            source: "",
            booruPostId: "",
            booruLastUpdate: "",
            md5: "",
            postedDateTime: "",
            postedById: "",
            postedByUsername: "",
            rating: "",
            ratingSafeLevel: "",
            parentId: default,
            parentMd5: default,
            childs: Array.Empty<string>(),
            pools: generatedTags,
            tags: Array.Empty<MetaParsingTagResults>(),
            notes: Array.Empty<Note>(),
            null);

        var resultTagsValues = obj.GetMetaTags().ToArray();

        foreach (var tag in generatedTags)
        {
            resultTagsValues.Count(x => x.Tag == "Pool" && x.Value == tag).Should().Be(1);
        }
    }

    [Fact]
    public void ShouldReturnChildren()
    {
        var generatedTags = Enumerable.Range(0, 10).Select(x => $"tag{x}").ToArray();

        var obj = new MetaParsingResults(
            source: "",
            booruPostId: "",
            booruLastUpdate: "",
            md5: "",
            postedDateTime: "",
            postedById: default,
            postedByUsername: default,
            rating: "",
            ratingSafeLevel: "",
            parentId: default,
            parentMd5: default,
            childs: generatedTags,
            pools: Array.Empty<string>(),
            tags: Array.Empty<MetaParsingTagResults>(),
            notes: Array.Empty<Note>(),
            null);
            
        var resultTagsValues = obj.GetMetaTags().ToArray();

        foreach (var tag in generatedTags)
        {
            resultTagsValues.Count(x => x.Tag == "Child" && x.Value == tag).Should().Be(1);
        }
    }

    [Fact]
    public void ShouldReturnUgoiraFrameData()
    {
        var generatedTags = Enumerable.Range(0, 10).Select(x => $"tag{x}").ToArray();

        var obj = new MetaParsingResults(
            source: "",
            booruPostId: "",
            booruLastUpdate: "",
            md5: "",
            postedDateTime: "",
            postedById: "",
            postedByUsername: default,
            rating: "",
            ratingSafeLevel: "",
            parentId: default,
            parentMd5: default,
            childs: generatedTags,
            pools: Array.Empty<string>(),
            tags: Array.Empty<MetaParsingTagResults>(),
            notes: Array.Empty<Note>(),
            new UgoiraFrameData("jpeg", new[] { new FrameData(10, "file") }));
            
        var tags = obj.GetMetaTags().ToArray();

        tags.FirstOrDefault(x => x.Tag == "UgoiraFrameData").Should().NotBeNull();
        tags.FirstOrDefault(x => x.Tag == "UgoiraFrameData").Value.Should().NotBeEmpty();
    }
}
