using FluentAssertions;
using Flurl.Http.Configuration;
using Imouto.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;
using SankakuSettings = ImoutoRebirth.Arachne.Infrastructure.Models.Settings.SankakuSettings;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure;

public class YandereLoaderTests
{
    [Fact(Skip = "Only local run")]
    public async Task ShouldHaveAllTags()
    {
        // arrange
        var settings = new SankakuSettings
        {
            Login = "testuser159",
            Password = "testuser159",
            Delay = 6000
        };

        var fabric = new SankakuLoaderFabric(settings,
            new SankakuAuthManager(new MemoryCache(new MemoryCacheOptions()), Options.Create(
                    new Imouto.BooruParser.Implementations.Sankaku.SankakuSettings()
                    {
                        Login = settings.Login,
                        Password = settings.Password,
                        PauseBetweenRequestsInMs = settings.Delay
                    }),
                new PerBaseUrlFlurlClientFactory()), new PerBaseUrlFlurlClientFactory());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostByMd5Async("4c58f8a682ccdf6eef5b3ed28678c71d");

        // assert
        post!.Tags.Should().Contain(x => x.Name == "tattoo");
    }

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
        post.ChildrenIds.Select(x => x.Id).Should().BeEquivalentTo([709435, 753883, 801669]);
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
