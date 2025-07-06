using ImoutoRebirth.Lamia.Domain.FileAggregate;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta.Providers;

internal class VideoMetadataProvider : IMetadataForFileProvider
{
    private readonly IFFmpegAccessor _ffmpegAccessor;

    public VideoMetadataProvider(IFFmpegAccessor ffmpegAccessor) => _ffmpegAccessor = ffmpegAccessor;

    public bool IsProviderFor(string filePath) => filePath.IsVideo();

    public async Task<FileMetadata> GetMetadata(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var size = fileInfo.Length;
        var extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();

        var mediaInfo = await _ffmpegAccessor.GetMediaInfo(filePath);
        var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
        var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

        if (videoStream is null)
            throw new InvalidDataException("File recognised as video but video stream is missing.");

        var resolution = new Resolution(videoStream.Width, videoStream.Height);

        var aspectRatio = AspectRatioCalculator.DetermineAspectRatio(resolution);

        var duration = mediaInfo.Duration;
        var hasAudio = audioStream != null;
        var codec = videoStream.Codec;
        var audioCodec = audioStream?.Codec;
        var framerate = videoStream.Framerate;

        return new FileMetadata(
            SizeInBytes: size,
            MediaFileType: MediaFileType.Video,
            Extension: extension,
            Resolution: resolution,
            AspectRatio: aspectRatio,
            Duration: duration,
            HasAudio: hasAudio,
            AnimationFrames: null,
            ArchiveFilesCount: null,
            Codec: codec,
            AudioCodec: audioCodec,
            Framerate: framerate,
            MaybeUgoira: false);
    }
}
