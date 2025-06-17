using FluentAssertions;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.Schale;
using Microsoft.Extensions.Logging.Abstractions;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure.Schale;

public class SchaleMetadataProviderTests
{
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
        await Verify(result.Single());
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

        await Verify(metadata.Tags);
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
