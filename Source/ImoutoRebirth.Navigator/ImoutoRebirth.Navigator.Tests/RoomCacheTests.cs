using System.Net.Http;
using AutoMapper;
using AwesomeAssertions;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services.Tags;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using SearchTag = ImoutoRebirth.Navigator.Services.Tags.Model.SearchTag;
using SearchType = ImoutoRebirth.Navigator.Services.Tags.Model.SearchType;

namespace ImoutoRebirth.Navigator.Tests;

public class RoomCacheTests : IDisposable
{
    private readonly MemoryCache _memory = new(new MemoryCacheOptions());
    private readonly TestClient _client = new();
    private readonly RoomCache _cache;
    private readonly Guid _firstFileId = Guid.NewGuid();
    private readonly Guid _secondFileId = Guid.NewGuid();

    public RoomCacheTests()
    {
        _cache = new RoomCache(_client, _memory);
        _cache.Clear();
    }

    [Fact]
    public async Task GetFilesFromCollection_ShouldShareModelsAndIdsWithSubsequentSearches()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>(
        [
            new(_firstFileId, "first.jpg"),
            new(_secondFileId, "second.jpg")
        ]);

        var loaded = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);
        var reordered = await _cache.GetFilesByIds([_secondFileId, _firstFileId]);
        var ids = await _cache.GetIds(null, null, null);

        loaded.Select(x => x.Id).Should().Equal(_firstFileId, _secondFileId);
        reordered.First().Should().BeSameAs(loaded.Last());
        ids.Should().Equal(_firstFileId, _secondFileId);
        _client.SummaryQueries.Should().ContainSingle();
        var tail = _client.IdQueries.Should().ContainSingle().Which;
        tail.Skip.Should().Be(2);
        tail.Count.Should().BeNull();
    }

    [Fact]
    public async Task GetFilesFromCollection_ShouldReuseModelsAndCheckOnlyNewIds_WhenCached()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>([new(_firstFileId, "first.jpg")]);
        var first = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);

        var second = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);

        second.Should().ContainSingle().Which.Should().BeSameAs(first.Single());
        _client.SummaryQueries.Should().ContainSingle();
        _client.IdQueries.Should().ContainSingle().Which.Skip.Should().Be(1);
    }

    [Fact]
    public async Task GetFilesByIds_ShouldLoadMissingModelsOnceAndPreserveRequestedOrder()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>(
        [
            new(_firstFileId, "first.jpg"),
            new(_secondFileId, "second.jpg")
        ]);

        var result = await _cache.GetFilesByIds([_secondFileId, _firstFileId, _secondFileId]);
        var empty = await _cache.GetFilesByIds([]);

        result.Select(x => x.Id).Should().Equal(_secondFileId, _firstFileId, _secondFileId);
        empty.Should().BeEmpty();
        _client.SummaryQueries.Should().ContainSingle().Which.CollectionFileIds.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetIds_ShouldStoreAppendedIdsAndAdvanceTheNextRefreshOffset()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>([new(_firstFileId, "first.jpg")]);
        await _cache.GetFilesFromCollection(null, 0, int.MaxValue);
        _client.Ids = query => query.Skip == 1 ? [_secondFileId] : [];

        var firstRefresh = await _cache.GetIds(null, null, null);
        var secondRefresh = await _cache.GetIds(null, null, null);

        firstRefresh.Should().Equal(_firstFileId, _secondFileId);
        secondRefresh.Should().Equal(_firstFileId, _secondFileId);
        _client.IdQueries.Select(x => x.Skip).Should().Equal(1, 2);
    }

    [Fact]
    public async Task Clear_ShouldInvalidateModelsAndCollectionIds()
    {
        var path = "old.jpg";
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>([new(_firstFileId, path)]);
        await _cache.GetFilesFromCollection(null, 0, int.MaxValue);
        path = "renamed.jpg";

        _cache.Clear();
        var reloaded = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);

        reloaded.Should().ContainSingle().Which.Path.Should().Be("renamed.jpg");
        _client.SummaryQueries.Should().HaveCount(2);
        _client.IdQueries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetFilesFromCollection_ShouldPassCancellationToTheApi()
    {
        using var cts = new CancellationTokenSource();
        _client.Summaries = (_, ct) =>
        {
            ct.Should().Be(cts.Token);
            return Task.FromCanceled<IReadOnlyCollection<CollectionFileSummary>>(ct);
        };
        cts.Cancel();

        var load = () => _cache.GetFilesFromCollection(null, 0, int.MaxValue, cts.Token);

        await load.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SearchFiles_ShouldLoadOneBatchAndUseTheServerCounter_WhenUnfiltered()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>([new(_firstFileId, "first.jpg")]);
        _client.Count = 4;
        var service = new FileService(
            new Mock<IMapper>().Object,
            new FilesClient(new HttpClient()),
            _client,
            _cache);

        var result = await service.SearchFiles(null, [], int.MaxValue, 0, default);
        var count = await service.CountFiles(null, [], default);

        result.Continue.Should().BeFalse();
        count.Should().Be(4);
        _client.IdQueries.Should().BeEmpty();
        _client.SummaryQueries.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchFiles_ShouldIntersectRoomIdsAndReuseModels_WhenTagsAreIncluded()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>(
        [
            new(_firstFileId, "first.jpg"),
            new(_secondFileId, "second.jpg")
        ]);
        var loaded = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);
        _client.Ids = query => query.Skip.GetValueOrDefault() == 0 ? [_secondFileId, _firstFileId] : [];
        var lilin = new Mock<FilesClient>(new HttpClient());
        lilin.Setup(x => x.SearchFilesFastAsync(It.IsAny<SearchFilesFastQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_firstFileId, Guid.NewGuid()]);
        var service = new FileService(CreateMapper(), lilin.Object, _client, _cache);
        var collectionId = Guid.NewGuid();
        SearchTag[] tags = [CreateSearchTag(SearchType.Include)];

        var result = await service.SearchFiles(collectionId, tags, int.MaxValue, 0, default);
        var count = await service.CountFiles(collectionId, tags, default);

        result.Files.Should().ContainSingle().Which.Should().BeSameAs(loaded.First());
        count.Should().Be(1);
        _client.SummaryQueries.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchFiles_ShouldReuseModelsAndKeepTheFilteredCounter_WhenTagsAreExcluded()
    {
        _client.Summaries = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>(
        [
            new(_firstFileId, "first.jpg"),
            new(_secondFileId, "second.jpg")
        ]);
        var loaded = await _cache.GetFilesFromCollection(null, 0, int.MaxValue);
        var lilin = new Mock<FilesClient>(new HttpClient());
        lilin.Setup(x => x.FilterFilesAsync(It.IsAny<FilterFilesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_secondFileId]);
        lilin.Setup(x => x.CountFilterFilesAsync(It.IsAny<FilterFilesCountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        var service = new FileService(CreateMapper(), lilin.Object, _client, _cache);
        SearchTag[] tags = [CreateSearchTag(SearchType.Exclude)];

        var result = await service.SearchFiles(null, tags, int.MaxValue, 0, default);
        var count = await service.CountFiles(null, tags, default);

        result.Files.Should().ContainSingle().Which.Should().BeSameAs(loaded.Last());
        count.Should().Be(1);
        _client.SummaryQueries.Should().ContainSingle();
    }

    private static IMapper CreateMapper()
    {
        var mapper = new Mock<IMapper>();
        mapper.Setup(x => x.Map<List<TagSearchEntry>>(It.IsAny<object>())).Returns([]);
        return mapper.Object;
    }

    private static SearchTag CreateSearchTag(SearchType searchType)
        => new(
            new(Guid.NewGuid(), "tag", new(Guid.NewGuid(), "type", 0), [], false, false, 0),
            null,
            searchType);

    public void Dispose()
    {
        _cache.Clear();
        _memory.Dispose();
    }

    private sealed class TestClient() : CollectionFilesClient(new HttpClient())
    {
        public List<CollectionFilesQuery> SummaryQueries { get; } = [];
        public List<CollectionFilesQuery> IdQueries { get; } = [];
        public Func<CollectionFilesQuery, CancellationToken, Task<IReadOnlyCollection<CollectionFileSummary>>> Summaries { get; set; }
            = (_, _) => Task.FromResult<IReadOnlyCollection<CollectionFileSummary>>([]);
        public Func<CollectionFilesQuery, IReadOnlyCollection<Guid>> Ids { get; set; } = _ => [];
        public int Count { get; set; }

        public override Task<IReadOnlyCollection<CollectionFileSummary>> SearchCollectionFileSummariesAsync(
            CollectionFilesQuery body,
            CancellationToken ct)
        {
            SummaryQueries.Add(body);
            return Summaries(body, ct);
        }

        public override Task<IReadOnlyCollection<Guid>> SearchCollectionFileIdsAsync(
            CollectionFilesQuery body,
            CancellationToken ct)
        {
            IdQueries.Add(body);
            return Task.FromResult(Ids(body));
        }

        public override Task<int> CountCollectionFilesAsync(CollectionFilesQuery body, CancellationToken ct)
            => Task.FromResult(Count);
    }
}
