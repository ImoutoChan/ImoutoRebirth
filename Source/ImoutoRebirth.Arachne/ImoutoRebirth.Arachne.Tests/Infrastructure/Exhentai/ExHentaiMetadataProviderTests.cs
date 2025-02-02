using FluentAssertions;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure.ExHentai;

public class ExHentaiMetadataProviderTests : IClassFixture<TestConfiguration>
{
    private readonly Mock<ILogger<ExHentaiMetadataProvider>> _loggerMock;
    private readonly ExHentaiAuthConfig _authConfig;

    public ExHentaiMetadataProviderTests(TestConfiguration configuration)
    {
        _loggerMock = new Mock<ILogger<ExHentaiMetadataProvider>>();
        _authConfig = configuration.GetExHentaiAuthConfig();
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata1()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);

        // act
        var result = await provider.SearchMetadataAsync("[Mint no Chicchai Oana (Mint Muzzlini)] Toxic JK Netorare Jigo Houkoku... [English] [Solid Rose]");

        // assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new FoundMetadata(
                    "Toxic JK Netorare Jigo Houkoku...",
                    ["Mint no Chicchai Oana (Mint Muzzlini)"],
                    "Unknown",
                    [
                        "language:english",
                        "language:translated",
                        "parody:original",
                        "group:mint no chicchai oana",
                        "male:blackmail",
                        "male:netorare",
                        "male:rape",
                        "male:sole male",
                        "female:big breasts",
                        "female:blackmail",
                        "female:blowjob",
                        "female:femdom",
                        "female:schoolgirl uniform",
                        "female:twintails",
                        "other:mosaic censorship"
                    ],
                    ["english", "translated"],
                    4.53m,
                    "https://ehgt.org/w/01/493/96471-sdkeqwcd.webp",
                    2921730,
                    "d97ebe632e"));
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata2()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);

        // act
        var result = await provider.SearchMetadataAsync("(C78) [Kairanban (Bibi)] Benten Kairaku 16 Moshimo Kare Ga Boketa Nara (Bleach) [English]");

        // assert
        result.Should().BeEquivalentTo([
                new FoundMetadata(
                    "Benten Kairaku 16 Moshimo Kare Ga Boketa Nara",
                    ["Kairanban (Bibi)"],
                    "C78",
                    [
                        "language:chinese", "language:translated", "parody:bleach", "character:rangiku matsumoto", "group:kairanban", "artist:bibi", "artist:emine kendama", "male:old man", "female:big breasts"
                    ],
                    ["chinese", "translated"],
                    4.48m,
                    "https://ehgt.org/e4/16/e41614d8449685d860f5f9e534fd8adeccdc92d4-1512400-1225-1750-jpg_250.jpg",
                    861082,
                    "db0866f23e"),
                new FoundMetadata(
                    "Benten Kairaku 16 Moshimo Kare Ga Boketa Nara",
                    ["Kairanban (Bibi)"],
                    "C78",
                    [
                        "parody:bleach", "character:genryusai shigekuni yamamoto", "character:rangiku matsumoto", "group:kairanban", "artist:bibi", "artist:emine kendama", "male:dilf", "male:old man", "male:scar", "female:big breasts", "female:breast feeding", "female:fingering", "female:milf", "female:nakadashi"
                    ],
                    ["japanese"],
                    4.54m,
                    "https://ehgt.org/5c/71/5c714491694f2ab3608a26d718128a61b31ab690-3338736-1225-1750-jpg_250.jpg",
                    829074,
                    "198e5a4e4f"),
                ]);
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata3()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);

        // act
        var result = await provider.SearchMetadataAsync("[mamaloni] Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko");

        // assert
        result.Should().BeEquivalentTo([
                new FoundMetadata(
                    "Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko | 처음 구입한 로터로 클리자위 기억해 버린 소녀",
                    ["mamaloni"],
                    "Unknown",
                    [
                        "language:korean", "language:translated", "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship"
                    ],
                    ["korean", "translated"],
                    4.53m,
                    "https://ehgt.org/w/01/582/61545-epzeywo7.webp",
                    3083613,
                    "25faded29b"),
                new FoundMetadata(
                    "Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko | 用第一次買的成人玩具學會了超刺激陰蒂自慰的女孩子",
                    ["mamaloni"],
                    "Unknown",
                    [
                        "language:chinese", "language:translated", "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship", "other:no penetration"
                    ],
                    ["chinese", "translated"],
                    4.70m,
                    "https://ehgt.org/w/01/464/24020-h7kd5w1f.webp",
                    2873324,
                    "e949c2765d"),
                new FoundMetadata(
                    "Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko",
                    ["mamaloni"],
                    "Unknown",
                    [
                        "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship"
                    ],
                    ["japanese"],
                    4.52m,
                    "https://ehgt.org/w/01/460/42204-pqxivp13.webp",
                    2867085,
                    "7fa0159874"),
                ]);
    }

    [Fact]
    public async Task SearchMetadataAsync_EmptyGalleryName_ReturnsEmptyAndLogsWarning()
    {
        // Arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);

        // Act
        var result = await provider.SearchMetadataAsync("");

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("[Author] Title (Publisher)", "Title", "Author", "Unknown")]
    [InlineData("Title", "Title", "Unknown", "Unknown")]
    [InlineData("[Author] Title", "Title", "Author", "Unknown")]
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
    public async Task GetGalleryIds_ValidHtml_ReturnsParsedIds()
    {
        // Arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);

        // Act
        var result = await provider.GetGalleryIds("[mamaloni] Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko");

        // Assert
        result.Should().BeEquivalentTo([
            new [] { "3083613", "25faded29b" },
            new [] { "2873324", "e949c2765d" },
            new [] { "2867085", "7fa0159874" },
        ]);
    }
}
