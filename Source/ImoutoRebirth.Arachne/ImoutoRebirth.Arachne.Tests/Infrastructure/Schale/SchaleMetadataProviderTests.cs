using FluentAssertions;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Schale;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure.Schale;

public class SchaleMetadataProviderTests
{
    private readonly Mock<ILogger<SchaleMetadataProvider>> _loggerMock = new();
    private readonly IFlurlClientCache _flurlClientCache = new FlurlClientCache();

    /// <summary>
    /// Works only with vpn from dev pc.
    /// </summary>
    [Fact]
    public async Task AvailabilityChecker_ShouldReturnAvailable()
    {
        // arrange
        var provider = new SchaleMetadataProvider(_flurlClientCache);

        // act
        var available = await provider.IsAvailable(CancellationToken.None);

        // assert
        available.Should().BeTrue();
    }

    [Fact]
    public async Task MetadataProvider_WithSearchString_ReturnsGallery()
    {
        // arrange
        var provider = new SchaleMetadataProvider(_flurlClientCache);

        // act
        var result = await provider
            .SearchAsync("[Ootori Mahiro] My Wife Started Experimenting -Desperate Housewife-");

        // assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new SchaleMetadata(
                    "My Wife Started Experimenting -Desperate Housewife-",
                    [new GalleryTag(SchaleType.Artist, "ootori mahiro")],
                    "",
                    [
                        new GalleryTag(SchaleType.Language, "english"),
                        new GalleryTag(SchaleType.Language, "translated"),
                        new GalleryTag(SchaleType.Artist, "ootori mahiro"),
                        new GalleryTag(SchaleType.Tag, "ahegao"),
                        new GalleryTag(SchaleType.Tag, "big penis"),
                        new GalleryTag(SchaleType.Tag, "blowjob"),
                        new GalleryTag(SchaleType.Tag, "bukkake"),
                        new GalleryTag(SchaleType.Tag, "fingering"),
                        new GalleryTag(SchaleType.Tag, "lingerie"),
                        new GalleryTag(SchaleType.Tag, "masturbation"),
                        new GalleryTag(SchaleType.Tag, "nakadashi"),
                        new GalleryTag(SchaleType.Female, "busty"),
                        new GalleryTag(SchaleType.Female, "harem"),
                        new GalleryTag(SchaleType.Female, "impregnation"),
                        new GalleryTag(SchaleType.Female, "milf"),
                        new GalleryTag(SchaleType.Female, "paizuri"),
                        new GalleryTag(SchaleType.Mixed, "group"),
                    ],
                    [
                        new GalleryTag(SchaleType.Language, "english"),
                        new GalleryTag(SchaleType.Language, "translated")
                    ],
                    24929,
                    "221a6bd176fe",
                    new DateTimeOffset(2025, 2, 2, 1, 11, 28, 250, TimeSpan.Zero)
                ));
    }

    [Fact]
    public async Task SearchEngine_WithSearchString_ReturnsGallery()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = int.MaxValue;

        var provider = new SchaleMetadataProvider(_flurlClientCache);
        var engine = new SchaleSearchEngine(provider, NullLogger<SchaleSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "[Ootori Mahiro] My Wife Started Experimenting -Desperate Housewife-.cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.FileIdFromSource.Should().Be("24929|221a6bd176fe");
        metadata.IsFound.Should().BeTrue();

        var expectedTags = new [] {
            new Tag("LocalMeta", "BooruPostId", "24929|221a6bd176fe"),
            new Tag("LocalMeta", "Md5", ""),
            new Tag("Copyright", "my wife started experimenting -desperate housewife-"),

            new Tag("LocalMeta", "PostedDateTime", "02.02.2025 01:11:28"),

            new Tag("Meta", "english"),
            new Tag("Meta", "translated"),

            new Tag("Artist", "ootori mahiro"),

            new Tag("General", "ahegao"),
            new Tag("General", "big penis"),
            new Tag("General", "blowjob"),
            new Tag("General", "bukkake"),
            new Tag("General", "fingering"),
            new Tag("General", "lingerie"),
            new Tag("General", "masturbation"),
            new Tag("General", "nakadashi"),

            new Tag("General", "busty"),
            new Tag("General", "harem"),
            new Tag("General", "impregnation"),
            new Tag("General", "milf"),
            new Tag("General", "impregnation"),
            new Tag("General", "paizuri"),
            new Tag("General", "female:busty"),
            new Tag("General", "female:harem"),
            new Tag("General", "female:impregnation"),
            new Tag("General", "female:milf"),
            new Tag("General", "female:impregnation"),
            new Tag("General", "female:paizuri"),

            new Tag("General", "group")
        };

        foreach (var expectedTag in expectedTags)
        {
            metadata.Tags.Should().ContainEquivalentOf(expectedTag);
        }
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata5()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new SchaleMetadataProvider(_flurlClientCache);
        var engine = new SchaleSearchEngine(provider, NullLogger<SchaleSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C94) [Castella Tamago (Piyopiyo)] Albedo wa Goshujin-sama no Yume o Miru ka Do Albedo Dream of Master (Overlord) [English] [Digital].cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.IsFound.Should().BeFalse();
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata6()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = int.MaxValue;

        var provider = new SchaleMetadataProvider(_flurlClientCache);
        var engine = new SchaleSearchEngine(provider, NullLogger<SchaleSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C81) [NIKOPONDO (Aoyama Reo)] DG - Daddy’s Girl Vol. 8 [English] [Whale].cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.IsFound.Should().BeFalse();
    }
}
