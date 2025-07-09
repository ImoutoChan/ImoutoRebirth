using System.Diagnostics;

namespace ImoutoRebirth.Lamia.Domain.FileAggregate;

public record FileMetadata(
    long SizeInBytes,
    MediaFileType MediaFileType,
    string? Extension,
    Resolution? Resolution,
    AspectRatio? AspectRatio,
    TimeSpan? Duration,
    bool? HasAudio,
    int? AnimationFrames,
    int? ArchiveFilesCount,
    string? Codec,
    string? AudioCodec,
    double? Framerate,
    bool MaybeUgoira)
{
    public IEnumerable<(string tagName, string? tagValue)> ExtractTags()
    {
        yield return ("size", SizeInBytes.ToString());
        yield return ("type:" + MediaFileType.ToString().ToLower(), null);

        if (MediaFileType == MediaFileType.Video)
        {
            yield return ("animated", null);
            yield return ("video", null);
        }

        if (!string.IsNullOrEmpty(Extension))
            yield return (Extension.ToLower(), null);

        if (Resolution != null)
        {
            yield return ($"{Resolution.Width}x{Resolution.Height}", null);
            yield return ("width", Resolution.Width.ToString());
            yield return ("height", Resolution.Height.ToString());
        }

        if (AspectRatio != null)
        {
            yield return (AspectRatio.Orientation.ToString().ToLower(), null);

            if (AspectRatio.Standard != StandardAspectRatio.Other)
                yield return ("aspect ratio:" + AspectRatio.Standard.ToString().Trim('_'), null);

            if (AspectRatio.Custom != null)
                yield return ("aspect ratio:" + AspectRatio.Custom.Value.ToString("0.00"), null);
        }

        if (Duration != null)
        {
            yield return ("duration", FormatDuration(Duration.Value));

            if (Duration.Value.TotalSeconds <= 6)
                yield return ("duration 5s", null);
            else if (Duration.Value.TotalSeconds <= 12)
                yield return ("duration 10s", null);
            else if (Duration.Value.TotalSeconds <= 35)
                yield return ("duration 30s", null);
            else if (Duration.Value.TotalSeconds <= 70)
                yield return ("duration 1m", null);
            else if (Duration.Value.TotalSeconds <= 60 * 3 + 30)
                yield return ("duration 3m", null);
            else if (Duration.Value.TotalMinutes <= 12)
                yield return ("duration 10m", null);
            else if (Duration.Value.TotalMinutes <= 40)
                yield return ("duration 30m", null);
            else if (Duration.Value.TotalMinutes <= 70)
                yield return ("duration 1h", null);
            else if (Duration.Value.TotalMinutes <= 140)
                yield return ("duration 2h", null);
            else if (Duration.Value.TotalMinutes <= 200)
                yield return ("duration 3h", null);
            else
                yield return ("duration 3h+", null);

        }

        if (HasAudio == true)
            yield return ("sound", null);

        if (AnimationFrames != null)
        {
            yield return ("animated", null);
            yield return ("animated frames", AnimationFrames.Value.ToString());
        }

        if (ArchiveFilesCount != null)
        {
            yield return ("archived", null);
            yield return ("archived files count", ArchiveFilesCount.Value.ToString());
        }

        if (!string.IsNullOrEmpty(Codec))
            yield return (Codec.ToLower(), null);

        if (!string.IsNullOrEmpty(AudioCodec))
            yield return ("audio " + AudioCodec.ToLower(), null);

        if (Framerate != null)
        {
            var fps = (int)Math.Round(Framerate.Value);
            yield return (fps + "fps", null);

            if (fps < 25)
                yield return ("10+fps", null);
            else if (fps < 55)
                yield return ("30+fps", null);
            else if (fps < 90)
                yield return ("60+fps", null);
            else if (fps < 120)
                yield return ("90+fps", null);
            else
                yield return ("120+fps", null);
        }

        if (MaybeUgoira)
            yield return ("maybe ugoira", null);
    }

    private static string FormatDuration(TimeSpan ts)
    {
        return ts.Hours > 0
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }
}

public enum MediaFileType
{
    Image,
    Animation,
    Video,
    Archive
}

[DebuggerDisplay("{Width}x{Height}")]
public record Resolution(int Width, int Height);
