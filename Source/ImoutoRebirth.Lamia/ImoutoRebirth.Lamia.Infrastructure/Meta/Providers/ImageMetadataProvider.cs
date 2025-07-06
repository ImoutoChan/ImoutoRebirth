using ImoutoRebirth.Lamia.Domain.FileAggregate;
using SixLabors.ImageSharp;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta.Providers;

internal class ImageMetadataProvider : IMetadataForFileProvider
{
    private readonly IFFmpegAccessor _ffmpegAccessor;

    public ImageMetadataProvider(IFFmpegAccessor ffmpegAccessor) => _ffmpegAccessor = ffmpegAccessor;

    public bool IsProviderFor(string filePath) => filePath.IsImage();

    public async Task<FileMetadata> GetMetadata(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var size = fileInfo.Length;
        var extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();

        var mediaInfo = await _ffmpegAccessor.GetMediaInfo(filePath);
        await using var fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var image = await Image.LoadAsync(fs);

        var resolution = new Resolution(image.Width, image.Height);
        var frames = image.Frames.Count;
        int? animationFrames = frames > 1 ? frames : null;
        var format = animationFrames.HasValue ? MediaFileType.Animation : MediaFileType.Image;
        var aspectRatio = AspectRatioCalculator.DetermineAspectRatio(resolution);

        return new FileMetadata(
            SizeInBytes: size,
            MediaFileType: format,
            Extension: extension,
            Resolution: resolution,
            AspectRatio: aspectRatio,
            Duration: null,
            HasAudio: false,
            AnimationFrames: animationFrames,
            ArchiveFilesCount: null,
            Codec: mediaInfo?.VideoStreams?.FirstOrDefault()?.Codec,
            AudioCodec: null,
            Framerate: null,
            MaybeUgoira: false);
    }
}
