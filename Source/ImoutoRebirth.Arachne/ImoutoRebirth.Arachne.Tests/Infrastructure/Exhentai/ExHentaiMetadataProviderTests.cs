using System.Runtime.CompilerServices;
using AwesomeAssertions;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure.ExHentai;

public static class VerifierStaticSettings
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifierSettings.DontScrubDateTimes();
    }
}

public class ExHentaiMetadataProviderTests : IClassFixture<TestConfiguration>
{
    private readonly Mock<ILogger<ExHentaiMetadataProvider>> _loggerMock;
    private readonly ExHentaiAuthConfig _authConfig;
    private readonly ExHentaiAuthConfig _emptyAuthConfig;
    private readonly IFlurlClientCache _flurlClientCache = new FlurlClientCache();

    public ExHentaiMetadataProviderTests(TestConfiguration configuration)
    {
        _loggerMock = new Mock<ILogger<ExHentaiMetadataProvider>>();
        _authConfig = configuration.GetExHentaiAuthConfig();
        _emptyAuthConfig = new ExHentaiAuthConfig(null, null, null, null);
    }

    /// <summary>
    /// Works only with vpn from dev pc.
    /// </summary>
    [Fact(Skip = "Manual debug")]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task AvailabilityChecker_ShouldReturnAvailable()
    {
        // arrange
        var providerAuth = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var providerWithoutAuth = new ExHentaiMetadataProvider(_flurlClientCache, _emptyAuthConfig, _loggerMock.Object);

        // act
        var withAuthAvailable = await providerAuth.IsAvailable(CancellationToken.None);
        var withoutAuthAvailable = await providerWithoutAuth.IsAvailable(CancellationToken.None);

        // assert
        withAuthAvailable.Should().BeFalse();
        withoutAuthAvailable.Should().BeTrue();
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata1()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);

        // act
        var result = await provider.DeepSearchMetadataAsync("[Mint no Chicchai Oana (Mint Muzzlini)] Toxic JK Netorare Jigo Houkoku... [English] [Solid Rose]");

        result.Should().HaveCount(1);
        await Verify(result.First());
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponseWithoutAuth_ReturnsParsedMetadata1()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _emptyAuthConfig, _loggerMock.Object);

        // act
        var result = await provider.DeepSearchMetadataAsync("[Mint no Chicchai Oana (Mint Muzzlini)] Toxic JK Netorare Jigo Houkoku... [English] [Solid Rose]");

        // assert
        result.Should().HaveCount(1);
        await Verify(result.First());
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponseWithAuth_ReturnsParsedMetadata5()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(new Image("", "[Akitsuki Itsuki] Where Love is Bound [English] =The Lost Light="));

        // assert
        result.Should().BeOfType<Metadata>();
        ((Metadata)result).IsFound.Should().BeTrue();
        ((Metadata)result).Tags.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponseWithAuth_ReturnsParsedMetadata6()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C86) [Hi-Per Pinch (clover)] Seisai wa Gomu-nashi Sex   Condomless Sex With My Wife (Sword Art Online) [English] {doujin-moe.us}.cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        ((Metadata)result).IsFound.Should().BeTrue();
        ((Metadata)result).Tags.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task Search_WithUnmodifiedName_ShouldFindMetadata()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C78) [BLUE BLOOD'S (BLUE BLOOD)] BLUE BLOOD'S Vol. 26 (Fate hollow ataraxia) [English] [EHCOVE].cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        ((Metadata)result).IsFound.Should().BeTrue();
        ((Metadata)result).Tags.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task Search_WithCleanedName_ShouldFindMetadata()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C101) [Yamadaya (TokutokuP)] Shameless Reincarnation - Cumming As Much As I Can After I Had My Soul Interchanged (Mushoku Tensei ~Isekai Ittara Honki Dasu~) [English] {Doujins.com}.cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        ((Metadata)result).IsFound.Should().BeTrue();
        ((Metadata)result).Tags.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata2()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);

        // act
        var result = await provider.DeepSearchMetadataAsync("(C78) [Kairanban (Bibi)] Benten Kairaku 16 Moshimo Kare Ga Boketa Nara (Bleach) [English]");

        // assert
        result.Should().HaveCount(2);
        await Verify(result);
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata3()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);

        // act
        var result = await provider.DeepSearchMetadataAsync("[mamaloni] Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko");

        // assert
        await Verify(result);
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata4()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "(C83) [LockerRoom (100 Yen Locker)] LR-03 (Sword Art Online) [Russian] [Witcher000].zip"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.FileIdFromSource.Should().Be("1007805|1d5c9d5deb");
        metadata.IsFound.Should().BeTrue();

        await Verify(metadata.Tags);
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata5()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "[Yuumyago] Let's Be Messy! (COMIC HOTMiLK 2014-09) [English] [Beya+drozetta]"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.IsFound.Should().BeTrue();
        metadata.Tags.Select(x => x.Name + x.Value).Should().AllSatisfy(x => x.Should().NotContain("&#"));
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata6()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "[Leticia Latex] Malpractice (Fixed).cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.IsFound.Should().BeTrue();
        metadata.Tags.Select(x => x.Name + x.Value).Should().AllSatisfy(x => x.Should().NotContain("&#"));
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata7()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(
            new Image(
                "",
                "[Hibon (Itami)] Futanari-san to Nonke-san _ Straight Girl Meets Futa [English] [2d-market.com] [Decensored] [Digital].cbz"));

        // assert
        result.Should().BeOfType<Metadata>();
        var metadata = (Metadata)result;
        metadata.IsFound.Should().BeTrue();
        metadata.Tags.Select(x => x.Name + x.Value).Should().AllSatisfy(x => x.Should().NotContain("&#"));
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task SearchMetadataAsync_EmptyGalleryName_ReturnsEmptyAndLogsWarning()
    {
        // Arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);

        // Act
        var result = await provider.DeepSearchMetadataAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("[Author] Title (Publisher)", "Title", "Author", "")]
    [InlineData("Title", "Title", "", "")]
    [InlineData("[Author] Title", "Title", "Author", "")]
    public void ParseTitle_ShouldExtractComponentsCorrectly(
        string input,
        string expectedTitle,
        string expectedAuthor,
        string expectedPublisher)
    {
        // Act
        var (title, author, publisher) = ExHentaiMetadataProvider.ParseTitle(input);

        // Assert
        title.Should().Be(expectedTitle);
        author.Should().Be(expectedAuthor);
        publisher.Should().Be(expectedPublisher);
    }

    [Theory]
    [InlineData(new[] { "language:english" }, new[] { "english" })]
    [InlineData(new[] { "tag1", "language:japanese" }, new[] { "japanese" })]
    [InlineData(new[] { "tag1" }, new[] { "japanese" })]
    [InlineData(new string[0], new[] { "japanese" })]
    public void DetectLanguages_ShouldReturnCorrectLanguages(
        IReadOnlyCollection<string> tags,
        IReadOnlyCollection<string> expected)
    {
        // Act
        var result = ExHentaiMetadataProvider.DetectLanguages(tags);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    [Trait("ExternalResourceRequired", "True")]
    [Trait("ExternalResource", "ExHentai")]
    public async Task GetGalleryIds_ValidHtml_ReturnsParsedIds()
    {
        // Arrange
        var provider = new ExHentaiMetadataProvider(_flurlClientCache, _authConfig, _loggerMock.Object);

        // Act
        var result = await provider.GetGalleryIds("[mamaloni] Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko");

        // Assert
        result.Should().BeEquivalentTo([
            new [] { "3083613", "25faded29b" },
            new [] { "2873324", "e949c2765d" },
            new [] { "2867085", "7fa0159874" },
            new [] { "3662611", "cf3350c65a" },
        ]);
    }
}
