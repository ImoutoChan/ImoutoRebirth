using System;
using System.Linq;
using FluentAssertions;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Models;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure
{
    public class MetaParsingResultsTests
    {
        [Fact]
        public void ShouldReturnAllSimpleTags()
        {
            var generatedTags = Enumerable.Range(0, 10).Select(x => $"tag{x}").ToArray();

            var obj = new MetaParsingResults(
                source: generatedTags[0],
                booruPostId: generatedTags[1],
                booruLastUpdate: generatedTags[2],
                md5: generatedTags[3],
                postedDateTime: generatedTags[4],
                postedById: generatedTags[5],
                postedByUsername: generatedTags[6],
                rating: generatedTags[7],
                parentId: generatedTags[8],
                parentMd5: generatedTags[9],
                childs: Array.Empty<string>(),
                pools: Array.Empty<string>(),
                tags: Array.Empty<(string TagType, string Tag, string[] Synonyms)>(),
                notes: Array.Empty<Note>());

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
                source: default,
                booruPostId: default,
                booruLastUpdate: default,
                md5: default,
                postedDateTime: default,
                postedById: default,
                postedByUsername: default,
                rating: default,
                parentId: default,
                parentMd5: default,
                childs: Array.Empty<string>(),
                pools: generatedTags,
                tags: Array.Empty<(string TagType, string Tag, string[] Synonyms)>(),
                notes: Array.Empty<Note>());

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
                source: default,
                booruPostId: default,
                booruLastUpdate: default,
                md5: default,
                postedDateTime: default,
                postedById: default,
                postedByUsername: default,
                rating: default,
                parentId: default,
                parentMd5: default,
                childs: generatedTags,
                pools: Array.Empty<string>(),
                tags: Array.Empty<(string TagType, string Tag, string[] Synonyms)>(),
                notes: Array.Empty<Note>());
            
            var resultTagsValues = obj.GetMetaTags().ToArray();

            foreach (var tag in generatedTags)
            {
                resultTagsValues.Count(x => x.Tag == "Child" && x.Value == tag).Should().Be(1);
            }
        }
    }
}
