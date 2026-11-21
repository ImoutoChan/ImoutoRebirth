using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImoutoRebirth.Navigator.ViewModel.ListEntries;
using ImoutoRebirth.Navigator.Utils;
using Xunit;

namespace ImoutoRebirth.Navigator.Tests;

public class DodjiEntryVMTests
{
    [Fact]
    public async Task ViewportChange_ShouldDiscardWaitingCoverAndAllowRescheduling()
    {
        using var queue = new AsyncThreadQueue();
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var workers = 0;
        async Task HoldWorker()
        {
            if (Interlocked.Increment(ref workers) == 2)
                started.SetResult();
            await release.Task;
        }

        queue.Add(HoldWorker);
        queue.Add(HoldWorker);
        try
        {
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var discarded = queue.Run(() => 1);
            queue.ClearQueue();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => discarded);
        }
        finally
        {
            release.SetResult();
        }
        Assert.Equal(2, await queue.Run(() => 2).WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Load_ShouldFinishAndPublishFrozenCoverWhenAvailable(bool hasCover)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.cbz");
        try
        {
            using (var archive = ZipFile.Open(path, ZipArchiveMode.Create))
            {
                if (hasCover)
                {
                    using (var text = new StreamWriter(archive.CreateEntry("notes.txt").Open()))
                        text.Write("notes");

                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(
                        BitmapSource.Create(2, 1, 96, 96, PixelFormats.Bgra32, null, new byte[8], 8)));
                    using var cover = archive.CreateEntry("cover.PNG").Open();
                    using var image = new MemoryStream();
                    encoder.Save(image);
                    image.Position = 0;
                    image.CopyTo(cover);
                }
            }

            var entry = new DodjiEntryVM(path, new Size(128, 128), null);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            entry.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(entry.IsLoading) && !entry.IsLoading)
                    finished.TrySetResult();
            };
            entry.Load();
            entry.Load();
            await finished.Task.WaitAsync(TimeSpan.FromSeconds(10));

            if (hasCover)
            {
                Assert.True(entry.Image?.IsFrozen);
                Assert.Equal(new Size(2, 1), entry.PixelSize);
            }
            else
            {
                Assert.Null(entry.Image);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Load_ShouldFinishWithoutCoverForMissingOrCorruptArchive(bool corrupt)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.cbz");
        try
        {
            if (corrupt)
                await File.WriteAllTextAsync(path, "invalid archive");

            var entry = new DodjiEntryVM(path, new Size(128, 128), null);
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            entry.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(entry.IsLoading) && !entry.IsLoading)
                    finished.TrySetResult();
            };
            entry.Load();
            await finished.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Null(entry.Image);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
