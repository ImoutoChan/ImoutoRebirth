using FluentAssertions;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ImoutoRebirth.Arachne.Tests.Infrastructure.ExHentai;

public class ExHentaiMetadataProviderTests : IClassFixture<TestConfiguration>
{
    private readonly Mock<ILogger<ExHentaiMetadataProvider>> _loggerMock;
    private readonly ExHentaiAuthConfig _authConfig;
    private readonly ExHentaiAuthConfig _emptyAuthConfig;
    private readonly ExHentaiAuthConfig _halfEmptyAuthConfig;

    public ExHentaiMetadataProviderTests(TestConfiguration configuration)
    {
        _loggerMock = new Mock<ILogger<ExHentaiMetadataProvider>>();
        _authConfig = configuration.GetExHentaiAuthConfig();
        _emptyAuthConfig = new ExHentaiAuthConfig(null, null, null, null);
        _halfEmptyAuthConfig = new ExHentaiAuthConfig(null, null, null, _authConfig.UserAgent);
    }

    /// <summary>
    /// Works only with vpn from dev pc.
    /// </summary>
    [Fact(Skip = "Manual debug")]
    public async Task AvailabilityChecker_ShouldReturnAvailable()
    {
        // arrange
        var providerAuth = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);
        var providerWithoutAuth = new ExHentaiMetadataProvider(_emptyAuthConfig, _loggerMock.Object);

        // act
        var withAuthAvailable = await providerAuth.IsAvailable(CancellationToken.None);
        var withoutAuthAvailable = await providerWithoutAuth.IsAvailable(CancellationToken.None);

        // assert
        withAuthAvailable.Should().BeFalse();
        withoutAuthAvailable.Should().BeTrue();
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
                    "",
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
                    "d97ebe632e",
                    "Doujinshi",
                    "JPMaximum_321",
                    45,
                    43866264,
                    false,
                    1,
                    "[ミントのちっちゃいお穴 (ミント・ムッツリーニ)] Toxic JK 寝取られ事後報告… [英訳]",
                    new DateTimeOffset(2024, 5, 17, 22, 22, 52, TimeSpan.Zero)
                ));
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponseWithoutAuth_ReturnsParsedMetadata1()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_emptyAuthConfig, _loggerMock.Object);

        // act
        var result = await provider.SearchMetadataAsync("[Mint no Chicchai Oana (Mint Muzzlini)] Toxic JK Netorare Jigo Houkoku... [English] [Solid Rose]");

        // assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new FoundMetadata(
                    "Toxic JK Netorare Jigo Houkoku...",
                    ["Mint no Chicchai Oana (Mint Muzzlini)"],
                    "",
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
                    "d97ebe632e",
                    "Doujinshi",
                    "JPMaximum_321",
                    45,
                    43866264,
                    false,
                    1,
                    "[ミントのちっちゃいお穴 (ミント・ムッツリーニ)] Toxic JK 寝取られ事後報告… [英訳]",
                    new DateTimeOffset(2024, 5, 17, 22, 22, 52, TimeSpan.Zero)));
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponseWithAuth_ReturnsParsedMetadata5()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);
        var engine = new ExHentaiSearchEngine(provider, NullLogger<ExHentaiSearchEngine>.Instance);

        // act
        var result = await engine.Search(new Image("", "[Akitsuki Itsuki] Where Love is Bound [English] =The Lost Light="));

        // assert
        result.Should().BeOfType<Metadata>();
        ((Metadata)result).IsFound.Should().BeTrue();
        ((Metadata)result).Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponseWithAuth_ReturnsParsedMetadata6()
    {
        // arrange
        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);
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
                    "db0866f23e",
                    "Doujinshi",
                    "少女与猫",
                    32,
                    23775809,
                    false,
                    0,
                    "(C78) [快乱版 (ビビ)] 弁天快楽 16 もしも彼がボケたなら (ブリーチ) [中国翻訳]",
                    new DateTimeOffset(2015, 10, 7, 0, 17, 19, TimeSpan.Zero)),
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
                    "198e5a4e4f",
                    "Doujinshi",
                    "walla*",
                    32,
                    41802581,
                    false,
                    0,
                    "(C78) [快乱版 (ビビ)] 弁天快楽 16 もしも彼がボケたなら (ブリーチ)",
                    new DateTimeOffset(2015, 7, 1, 12, 49, 47, TimeSpan.Zero)),
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
                    "",
                    [
                        "language:korean", "language:translated", "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship"
                    ],
                    ["korean", "translated"],
                    4.53m,
                    "https://ehgt.org/w/01/582/61545-epzeywo7.webp",
                    3083613,
                    "25faded29b",
                    "Doujinshi",
                    "땅콩머리",
                    14,
                    18725700,
                    false,
                    1,
                    "[mamaloni] はじめて買った大人のおもちゃでえぐいクリオナおぼえちゃった女の子 [韓国翻訳]",
                    new DateTimeOffset(2024, 10, 8, 1, 55, 34, TimeSpan.Zero)),
                new FoundMetadata(
                    "Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko | 用第一次買的成人玩具學會了超刺激陰蒂自慰的女孩子",
                    ["mamaloni"],
                    "",
                    [
                        "language:chinese", "language:translated", "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship", "other:no penetration"
                    ],
                    ["chinese", "translated"],
                    4.70m,
                    "https://ehgt.org/w/01/464/24020-h7kd5w1f.webp",
                    2873324,
                    "e949c2765d",
                    "Doujinshi",
                    "SZno1396",
                    15,
                    29019193,
                    false,
                    2,
                    "[Mamaloni] はじめて買った大人のおもちゃでえぐいクリオナおぼえちゃった女の子 [中国翻訳]",
                    new DateTimeOffset(2024, 3, 30, 11, 00, 02, TimeSpan.Zero)),
                new FoundMetadata(
                    "Hajimete Katta Otona no Omocha de Egui CliOna Oboechatta Onnanoko",
                    ["mamaloni"],
                    "",
                    [
                        "parody:original", "artist:mamaloni", "female:big clit", "female:clit stimulation", "female:masturbation", "female:multiple orgasms", "female:sex toys", "female:sole female", "female:solo action", "female:squirting", "other:full color", "other:mosaic censorship"
                    ],
                    ["japanese"],
                    4.52m,
                    "https://ehgt.org/w/01/460/42204-pqxivp13.webp",
                    2867085,
                    "7fa0159874",
                    "Doujinshi",
                    "111yami111",
                    14,
                    20637542,
                    false,
                    1,
                    "[mamaloni] はじめて買った大人のおもちゃでえぐいクリオナおぼえちゃった女の子",
                    new DateTimeOffset(2024, 03, 24, 19, 44, 41, TimeSpan.Zero)),
                ]);
    }

    [Fact]
    public async Task SearchMetadataAsync_ValidResponse_ReturnsParsedMetadata4()
    {
        // arrange
        AssertionConfiguration.Current.Formatting.MaxLines = Int32.MaxValue;

        var provider = new ExHentaiMetadataProvider(_authConfig, _loggerMock.Object);
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

        var expectedTags = new [] {
            new Tag("LocalMeta", "BooruPostId", "1007805|1d5c9d5deb"),
            new Tag("LocalMeta", "Md5", ""),
            new Tag("LocalMeta", "Score", "4.18"),
            new Tag("Copyright", "lr-03"),
            new Tag("General", "c83"),

            new Tag("General", "doujinshi"),
            new Tag("LocalMeta", "PostedByUsername", "drift49187"),
            new Tag("LocalMeta", "FilesCount", "15"),
            new Tag("LocalMeta", "FileSize", "58479334"),
            new Tag("LocalMeta", "PostedDateTime", "19.12.2016 08:55:58"),

            new Tag("Meta", "russian"),
            new Tag("Meta", "translated"),
            new Tag("Copyright", "sword art online"),
            new Tag("Character", "kirigaya kazuto"),
            new Tag("Character", "lyfa"),
            new Tag("Character", "kirigaya suguha"),
            new Tag("Artist", "locker room"),
            new Tag("Artist", "100yen locker"),

            new Tag("General", "male:elf"),
            new Tag("General", "elf"),
            new Tag("General", "male:sole male"),
            new Tag("General", "sole male"),
            new Tag("General", "male:virginity"),
            new Tag("General", "virginity"),

            new Tag("General", "female:big breasts"),
            new Tag("General", "big breasts"),
            new Tag("General", "female:cousin"),
            new Tag("General", "cousin"),
            new Tag("General", "female:elf"),
            new Tag("General", "female:masturbation"),
            new Tag("General", "masturbation"),
            new Tag("General", "female:tracksuit"),
            new Tag("General", "tracksuit"),

            new Tag("General", "incest"),
            new Tag("Meta", "full color")
        };

        foreach (var expectedTag in expectedTags)
        {
            metadata.Tags.Should().ContainEquivalentOf(expectedTag);
        }
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
