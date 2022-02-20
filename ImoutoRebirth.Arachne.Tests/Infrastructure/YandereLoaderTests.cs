using FluentAssertions;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure;

public class YandereLoaderTests
{
    [Fact]
    public async Task ShouldHaveMd5OnChildren()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new HttpClient());
        var loader = fabric.Create();
        
        // act
        var post = await loader.LoadPostAsync(801490);

        // assert
        var children = post.ChildrenIds
            .Select(x => x.Split(':'))
            .Where(x => x.Length == 2)
            .Select(x => (Id: x.First(), Md5: x.Last()))
            .ToList();

        children.Count.Should().Be(3);
        children.Select(x => x.Id).Should().BeEquivalentTo("709435", "753883", "801669");
        children.Select(x => x.Md5).Should().BeEquivalentTo("c1c4a14d4e058fe164482e86b8ea9b6b",
            "6a0e99ff9d228e0155ee5fd80f5217cb", "e038ea4fb55807d03f89e81df062409c");
    }
    
    [Fact]
    public async Task ShouldHaveMd5OnParentPost()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new HttpClient());
        var loader = fabric.Create();
        
        // act
        var post = await loader.LoadPostAsync(753883);

        // assert
        var parentInfo = post.ParentId.Split(':');
        var parentId = parentInfo[0];
        var parentMd5 = parentInfo[1];

        parentId.Should().Be("801490");
        parentMd5.Should().Be("67da0e3e0466397a72a714f895cd1024");
    }
    
    [Fact]
    public async Task ShouldWorkWithPostWithoutSiblings()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new HttpClient());
        var loader = fabric.Create();
        
        // act
        var post = await loader.LoadPostAsync(931867);

        // assert
        post.ParentId.Should().BeNull();
        post.ChildrenIds.Should().BeEmpty();
    }
}
