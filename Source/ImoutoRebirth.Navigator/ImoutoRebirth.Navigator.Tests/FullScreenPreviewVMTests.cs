using System.IO;
using System.Windows;
using ImoutoRebirth.Navigator.ViewModel;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using Xunit;

namespace ImoutoRebirth.Navigator.Tests;

public class FullScreenPreviewVMTests
{
    [Fact]
    public void NextPreview_ShouldRouteOnlyAnimatedPngToAnimationPlayerInBothDirections()
    {
        var still = new ImageEntryVM(Path.Combine(AppContext.BaseDirectory, "Resources", "Icon", "appicon.png"), new Size(128, 128), null);
        var animated = new ImageEntryVM(Path.Combine(AppContext.BaseDirectory, "Resources", "animated_png.png"), new Size(128, 128), null);
        var vm = new FullScreenPreviewVM(_ => throw new InvalidOperationException(), _ => Task.FromResult<IReadOnlyCollection<Services.Tags.Model.FileNote>>([]));

        vm.SetCurrentEntry(still, [still, animated]);
        Assert.Equal(still.Path, vm.ImagePath);
        Assert.Null(vm.PngPath);

        vm.NextPreviewCommand.Execute(true);
        Assert.Equal(animated.Path, vm.PngPath);
        Assert.Null(vm.ImagePath);

        vm.NextPreviewCommand.Execute(false);
        Assert.Equal(still.Path, vm.ImagePath);
        Assert.Null(vm.PngPath);
    }

    [Fact]
    public void NextPreview_ShouldSetAndClearGifSourceUri()
    {
        var gifPath = Path.Combine(Path.GetTempPath(), "animated # preview.gif");
        var gif = new ImageEntryVM(gifPath, new Size(128, 128), null);
        var still = new ImageEntryVM(
            Path.Combine(AppContext.BaseDirectory, "Resources", "Icon", "appicon.png"),
            new Size(128, 128),
            null);
        var vm = new FullScreenPreviewVM(
            _ => throw new InvalidOperationException(),
            _ => Task.FromResult<IReadOnlyCollection<Services.Tags.Model.FileNote>>([]));

        vm.SetCurrentEntry(gif, [gif, still]);
        Assert.Equal(new Uri(gifPath), vm.GifSourceUri);

        vm.NextPreviewCommand.Execute(true);
        Assert.Null(vm.GifSourceUri);

        vm.NextPreviewCommand.Execute(false);
        Assert.Equal(new Uri(gifPath), vm.GifSourceUri);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetCurrentEntry_ShouldKeepNavigationAvailableForMissingOrCorruptPng(bool corrupt)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.png");
        try
        {
            if (corrupt)
                File.WriteAllText(path, "invalid png");

            var entry = new ImageEntryVM(path, new Size(128, 128), null);
            var vm = new FullScreenPreviewVM(_ => throw new InvalidOperationException(), _ => Task.FromResult<IReadOnlyCollection<Services.Tags.Model.FileNote>>([]));
            vm.SetCurrentEntry(entry, [entry]);
            Assert.Same(entry, vm.CurrentEntry);
            Assert.Null(vm.PngPath);
            Assert.Null(vm.ImagePath);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
