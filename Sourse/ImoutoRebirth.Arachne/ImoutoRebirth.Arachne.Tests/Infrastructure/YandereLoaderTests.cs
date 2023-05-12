using FluentAssertions;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure;

public class YandereLoaderTests
{
    [Fact]
    public async Task ShouldHaveMd5OnChildren()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new PerBaseUrlFlurlClientFactory());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(801490);

        // assert
        post.ChildrenIds.Count.Should().Be(3);
        post.ChildrenIds.Select(x => x.Id).Should().BeEquivalentTo(new []{ 709435, 753883, 801669 });
        post.ChildrenIds.Select(x => x.Md5Hash).Should().BeEquivalentTo("c1c4a14d4e058fe164482e86b8ea9b6b",
            "6a0e99ff9d228e0155ee5fd80f5217cb", "e038ea4fb55807d03f89e81df062409c");
    }
    
    [Fact]
    public async Task ShouldHaveMd5OnParentPost()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new PerBaseUrlFlurlClientFactory());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(753883);

        // assert
        post.Parent.Should().NotBeNull();
        post.Parent!.Id.Should().Be(801490);
        post.Parent.Md5Hash.Should().Be("67da0e3e0466397a72a714f895cd1024");
    }
    
    [Fact]
    public async Task ShouldWorkWithPostWithoutSiblings()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new PerBaseUrlFlurlClientFactory());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(931867);

        // assert
        post.Parent.Should().BeNull();
        post.ChildrenIds.Should().BeEmpty();
    }
}
