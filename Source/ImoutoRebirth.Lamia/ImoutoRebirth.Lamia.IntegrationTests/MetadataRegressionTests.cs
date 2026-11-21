using ImoutoRebirth.Lamia.Domain.FileAggregate;
using ImoutoRebirth.Lamia.Infrastructure;
using ImoutoRebirth.Lamia.Infrastructure.Meta;
using ImoutoRebirth.Lamia.Infrastructure.Meta.Providers;
using Moq;
using Xabe.FFmpeg;
using Xunit;

namespace ImoutoRebirth.Lamia.IntegrationTests;

public class MetadataRegressionTests
{
    [Theory]
    [InlineData(2389, 1000, StandardAspectRatio._2_39, Orientation.Horizontal)]
    [InlineData(1000, 2400, StandardAspectRatio._2_40, Orientation.Vertical)]
    [InlineData(2460, 1000, StandardAspectRatio._2_44, Orientation.Horizontal)]
    [InlineData(2470, 1000, StandardAspectRatio.Other, Orientation.Horizontal)]
    public void ChoosesClosestStandardWithinTolerance(
        int width, int height, StandardAspectRatio expected, Orientation orientation)
    {
        var ratio = Assert.IsType<AspectRatio>(AspectRatioCalculator.DetermineAspectRatio(new Resolution(width, height)));

        Assert.Equal(expected, ratio.Standard);
        Assert.Equal(orientation, ratio.Orientation);
    }

    [Theory]
    [InlineData(3599, "59:59")]
    [InlineData(86399, "23:59:59")]
    [InlineData(86400, "24:00:00")]
    [InlineData(48 * 3600 + 7 * 60 + 9, "48:07:09")]
    public void DurationTagPreservesTotalHours(int seconds, string expected)
    {
        var metadata = new FileMetadata(
            SizeInBytes: 1,
            MediaFileType: MediaFileType.Video,
            Extension: "mp4",
            Resolution: null,
            AspectRatio: null,
            Duration: TimeSpan.FromSeconds(seconds),
            HasAudio: false,
            AnimationFrames: null,
            ArchiveFilesCount: null,
            Codec: null,
            AudioCodec: null,
            Framerate: null,
            Bpms: null,
            MaybeUgoira: false);

        var duration = Assert.Single(metadata.ExtractTags(), tag => tag.tagName == "duration");

        Assert.Equal(expected, duration.tagValue);
    }

    [Fact]
    public async Task AudioOnlyContainerEmitsAudioMetadataWithoutVideoTags()
    {
        var audio = Mock.Of<IAudioStream>(stream => stream.Codec == "aac");
        var media = new Mock<IMediaInfo>();
        media.SetupGet(info => info.VideoStreams).Returns([]);
        media.SetupGet(info => info.AudioStreams).Returns([audio]);
        media.SetupGet(info => info.Duration).Returns(TimeSpan.FromSeconds(4));
        var provider = new VideoMetadataProvider(new StubFFmpegAccessor(media.Object));
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "video.mp4");

        Assert.True(provider.IsProviderFor(path));
        var metadata = await provider.GetMetadata(path);
        var tags = metadata.ExtractTags().ToDictionary(tag => tag.tagName, tag => tag.tagValue);

        Assert.Equal(MediaFileType.Audio, metadata.MediaFileType);
        Assert.Equal("0:04", tags["duration"]);
        Assert.Contains("type:audio", tags.Keys);
        Assert.Contains("audio aac", tags.Keys);
        Assert.Contains("sound", tags.Keys);
        Assert.DoesNotContain("video", tags.Keys);
        Assert.Null(metadata.Resolution);
        Assert.Null(metadata.Framerate);
    }

    private class StubFFmpegAccessor(IMediaInfo mediaInfo) : IFFmpegAccessor
    {
        public Task<IMediaInfo> GetMediaInfo(string fileName) => Task.FromResult(mediaInfo);

        public IAsyncEnumerable<float[]> GetMonoAudioWindows(
            string fileName, int streamIndex, int sampleRate, TimeSpan window, TimeSpan maxDuration)
            => throw new InvalidOperationException("Short audio should bypass BPM detection.");
    }

    [Fact]
    public async Task UppercaseZipExtensionPreservesUgoiraFrames()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.ZIP");
        try
        {
            File.Copy(Path.Combine(AppContext.BaseDirectory, "Resources", "ugoira.zip"), path);
            var provider = new ArchiveMetadataProvider();

            Assert.True(provider.IsProviderFor(path));
            var metadata = await provider.GetMetadata(path);

            Assert.Equal(MediaFileType.Animation, metadata.MediaFileType);
            Assert.Equal(2, metadata.AnimationFrames);
            Assert.True(metadata.MaybeUgoira);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
