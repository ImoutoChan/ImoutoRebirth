using AwesomeAssertions;
using Flurl.Http.Configuration;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.Arachne.Infrastructure.LoaderFabrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
                new FlurlClientCache()), new FlurlClientCache());
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
        var fabric = new YandereLoaderFabric(new FlurlClientCache());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(801490);

        // assert
        await Verify(post.ChildrenIds);
    }
    
    [Fact]
    public async Task ShouldHaveMd5OnParentPost()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new FlurlClientCache());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(753883);

        // assert
        await Verify(post.Parent);
    }
    
    [Fact]
    public async Task ShouldWorkWithPostWithoutSiblings()
    {
        // arrange
        var fabric = new YandereLoaderFabric(new FlurlClientCache());
        var loader = fabric.Create();
        
        // act
        var post = await loader.GetPostAsync(931867);

        // assert
        post.Parent.Should().BeNull();
        post.ChildrenIds.Should().BeEmpty();
    }
}
