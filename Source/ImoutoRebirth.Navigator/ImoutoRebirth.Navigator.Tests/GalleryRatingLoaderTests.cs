using System.Net.Http;
using System.Windows;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Navigator.Services;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Moq;
using Xunit;

namespace ImoutoRebirth.Navigator.Tests;

public class GalleryRatingLoaderTests
{
    [Fact]
    public async Task LoadAsync_ShouldRequestVisibleIdsOnceAndCacheDefaultsForUntaggedFiles()
    {
        var rated = Entry(Guid.NewGuid());
        var duplicate = Entry(rated.DbId);
        var untagged = Entry(Guid.NewGuid());
        var local = Entry(null);
        using var http = new HttpClient();
        var client = new Mock<FilesClient>(http);
        client.Setup(x => x.GetFileRatingsAsync(It.IsAny<FileRatingsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new FileRating(rated.DbId!.Value, true, 4)]);
        var loader = new GalleryRatingLoader(client.Object);

        await loader.LoadAsync([rated, duplicate, untagged, local]);
        await loader.LoadAsync([rated, duplicate, untagged, local]);

        var query = Assert.IsType<FileRatingsQuery>(Assert.Single(client.Invocations).Arguments[0]);
        Assert.Equal(new[] { rated.DbId.Value, untagged.DbId!.Value }, query.FileIds);
        Assert.True(rated.IsFavorite && duplicate.IsFavorite);
        Assert.Equal(4, duplicate.Rating);
        Assert.True(untagged.IsRatingLoaded);
        Assert.False(untagged.IsFavorite);
        Assert.Equal(0, untagged.Rating);
        Assert.False(local.IsRatingLoaded);
    }

    [Fact]
    public async Task LoadAsync_ShouldIgnoreSupersededResponseEvenIfTransportFinishesIt()
    {
        var previous = Entry(Guid.NewGuid());
        var current = Entry(Guid.NewGuid());
        var oldResponse = new TaskCompletionSource<IReadOnlyCollection<FileRating>>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient();
        var client = new Mock<FilesClient>(http);
        client.SetupSequence(x => x.GetFileRatingsAsync(It.IsAny<FileRatingsQuery>(), It.IsAny<CancellationToken>()))
            .Returns(oldResponse.Task)
            .ReturnsAsync([new FileRating(current.DbId!.Value, false, 5)]);
        var loader = new GalleryRatingLoader(client.Object);

        var oldLoad = loader.LoadAsync([previous]);
        await loader.LoadAsync([current]);
        oldResponse.SetResult([new FileRating(previous.DbId!.Value, true, 2)]);
        await oldLoad;

        Assert.True(Assert.IsType<CancellationToken>(client.Invocations[0].Arguments[1]).IsCancellationRequested);
        Assert.False(previous.IsRatingLoaded);
        Assert.Equal(5, current.Rating);
    }

    private static ImageEntryVM Entry(Guid? id) => new("gallery.jpg", new Size(128, 128), id);
}
