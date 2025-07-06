using Microsoft.Extensions.Options;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace ImoutoRebirth.Lamia.Infrastructure;

internal interface IFFmpegAccessor
{
    Task<IMediaInfo> GetMediaInfo(string fileName);
}

internal class FFmpegAccessor : IFFmpegAccessor
{
    private static bool _isPrepared;
    private static readonly SemaphoreSlim Locker = new(1);

    private readonly IOptions<FFmpegOptions> _options;

    public FFmpegAccessor(IOptions<FFmpegOptions> options) => _options = options;

    private async ValueTask Prepare()
    {
        if (_isPrepared)
            return;

        await Locker.WaitAsync();
        try
        {
            if (_isPrepared)
                return;

            if (!string.IsNullOrWhiteSpace(_options.Value.Path))
            {
                var path = new DirectoryInfo(_options.Value.Path).FullName;

                if (FFmpeg.ExecutablesPath != path)
                    FFmpeg.SetExecutablesPath(path);
            }
            else
            {
                var defaultFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".ffmpeg");

                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, defaultFullPath);
                FFmpeg.SetExecutablesPath(defaultFullPath);
            }

            _isPrepared = true;
        }
        finally
        {
            Locker.Release();
        }
    }

    public async Task<IMediaInfo> GetMediaInfo(string fileName)
    {
        await Prepare();

        return await FFmpeg.GetMediaInfo(fileName);
    }
}

public class FFmpegOptions
{
    public required string Path { get; set; }
}
