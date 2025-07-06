using ImoutoRebirth.Lamia.Domain.FileAggregate;
using SharpCompress.Archives;
using SixLabors.ImageSharp;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta.Providers;

internal class ArchiveMetadataProvider : IMetadataForFileProvider
{
    public bool IsProviderFor(string filePath) => filePath.IsArchive();

    public async Task<FileMetadata> GetMetadata(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var size = fileInfo.Length;
        var extension = fileInfo.Extension.TrimStart('.').ToLowerInvariant();

        await using var fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var archive = ArchiveFactory.Open(fs);
        var archiveFileCount = archive.Entries.Count(e => !e.IsDirectory);
        var firstImage = archive.Entries.FirstOrDefault(x => x.Key?.IsImage() == true);

        var maybeUgoira = Path.GetExtension(filePath) == ".zip"
                          && (archive.Entries.Any(x => x.Key == "animation.json")
                              || archive.Entries.All(x
                                  => Path.GetFileNameWithoutExtension(x.Key)?.Length == 6
                                     && Path.GetFileNameWithoutExtension(x.Key)?.All(char.IsDigit) == true));

        FileMetadata? firstImageMetadata = null;
        if (firstImage is not null)
        {
            await using var stream = firstImage.OpenEntryStream();
            firstImageMetadata = await GetFirstImageMetadata(stream, firstImage.Key!, firstImage.Size);
        }

        if (maybeUgoira)
        {
            return new FileMetadata(
                SizeInBytes: size,
                MediaFileType: MediaFileType.Animation,
                Extension: extension,
                Resolution: firstImageMetadata?.Resolution,
                AspectRatio: firstImageMetadata?.AspectRatio,
                Duration: null,
                HasAudio: false,
                AnimationFrames: archive.Entries.Count(e => !e.IsDirectory && e.Key?.IsImage() == true),
                ArchiveFilesCount: archiveFileCount,
                Codec: null,
                AudioCodec: null,
                Framerate: null,
                MaybeUgoira: true);
        }

        return new FileMetadata(
            SizeInBytes: size,
            MediaFileType: MediaFileType.Archive,
            Extension: extension,
            Resolution: firstImageMetadata?.Resolution,
            AspectRatio: firstImageMetadata?.AspectRatio,
            Duration: null,
            HasAudio: false,
            AnimationFrames: firstImageMetadata?.AnimationFrames,
            ArchiveFilesCount: archiveFileCount,
            Codec: null,
            AudioCodec: null,
            Framerate: null,
            MaybeUgoira: false);
    }

    private static async Task<FileMetadata> GetFirstImageMetadata(Stream stream, string key, long size)
    {
        var extension = Path.GetExtension(key).TrimStart('.').ToLowerInvariant();

        using var image = await Image.LoadAsync(stream);

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
            Codec: null,
            AudioCodec: null,
            Framerate: null,
            MaybeUgoira: false);
    }
}
