using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Options;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace ImoutoRebirth.Lamia.Infrastructure;

internal interface IFFmpegAccessor
{
    Task<IMediaInfo> GetMediaInfo(string fileName);

    IAsyncEnumerable<float[]> GetMonoAudioWindows(
        string fileName,
        int streamIndex,
        int sampleRate,
        TimeSpan window,
        TimeSpan maxDuration);
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

    public async IAsyncEnumerable<float[]> GetMonoAudioWindows(
        string fileName,
        int streamIndex,
        int sampleRate,
        TimeSpan window,
        TimeSpan maxDuration)
    {
        await Prepare();

        using var process = Process.Start(new ProcessStartInfo(Path.Combine(FFmpeg.ExecutablesPath, "ffmpeg"))
        {
            ArgumentList =
            {
                "-nostdin", "-v", "error",
                "-t", ((int)maxDuration.TotalSeconds).ToString(),
                "-i", fileName,
                "-map", $"0:{streamIndex}",
                "-ac", "1", "-ar", sampleRate.ToString(), "-f", "f32le", "pipe:1"
            },
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;

        var output = process.StandardOutput.BaseStream;
        var buffer = new byte[(int)window.TotalSeconds * sampleRate * sizeof(float)];

        int read;
        while ((read = await output.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false)) > 0)
            yield return MemoryMarshal.Cast<byte, float>(buffer.AsSpan(0, read)).ToArray();

        await process.WaitForExitAsync();
    }
}

public class FFmpegOptions
{
    public required string Path { get; set; }
}
