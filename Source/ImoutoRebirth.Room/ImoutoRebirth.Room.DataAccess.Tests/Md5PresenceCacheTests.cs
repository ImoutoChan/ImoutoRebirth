using ImoutoRebirth.Room.DataAccess.Cache;
using Xunit;

namespace ImoutoRebirth.Room.DataAccess.Tests;

public class Md5PresenceCacheTests
{
    [Fact]
    public async Task FindPresentReusesPositiveAndNegativeAnswers()
    {
        using var cache = new Md5PresenceCache();
        var requests = new List<string[]>();

        Task<IReadOnlyCollection<string>> Load(IReadOnlyCollection<string> hashes, CancellationToken ct)
        {
            requests.Add(hashes.ToArray());
            return Task.FromResult<IReadOnlyCollection<string>>(hashes.Where(hash => hash == "abc").ToArray());
        }

        var first = await cache.FindPresent(["abc", "missing"], Load, CancellationToken.None);
        var second = await cache.FindPresent(["ABC", "MISSING", "new"], Load, CancellationToken.None);

        Assert.Equal(new[] { "abc" }, first);
        Assert.Equal(new[] { "ABC" }, second);
        Assert.Equal(new[] { "abc", "missing" }, requests[0]);
        Assert.Equal(new[] { "new" }, requests[1]);
        Assert.Equal(2, requests.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidationPreventsAnInFlightReadFromRefillingTheCache(bool clearAll)
    {
        using var cache = new Md5PresenceCache();
        var loading = new TaskCompletionSource<IReadOnlyCollection<string>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var inFlight = cache.FindPresent(["abc"], (_, _) => loading.Task, CancellationToken.None);

        if (clearAll)
        {
            cache.Clear();
        }
        else
        {
            cache.Remove("ABC");
        }

        loading.SetResult(["abc"]);
        await inFlight;

        var current = await cache.FindPresent(
            ["abc"],
            (_, _) => Task.FromResult<IReadOnlyCollection<string>>([]),
            CancellationToken.None);

        Assert.Empty(current);
    }
}
