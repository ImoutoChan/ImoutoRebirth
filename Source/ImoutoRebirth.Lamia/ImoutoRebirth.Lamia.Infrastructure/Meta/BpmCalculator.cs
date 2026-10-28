using SoundTouch;

namespace ImoutoRebirth.Lamia.Infrastructure.Meta;

internal static class BpmCalculator
{
    public const int SampleRate = 11025;
    public static readonly TimeSpan MinDuration = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan MaxDuration = TimeSpan.FromMinutes(30);

    private const int Tolerance = 2;
    private const int MinWindowsPerTrack = 2;

    public static async Task<IReadOnlyCollection<int>> DetermineBpms(IAsyncEnumerable<float[]> windows)
    {
        var windowsCount = 0;
        var bpms = new List<int>();

        await foreach (var samples in windows)
        {
            if (samples.Length < SampleRate * MinDuration.TotalSeconds)
                continue;

            windowsCount++;
            var detector = new BpmDetect(numChannels: 1, SampleRate);
            detector.InputSamples(samples, samples.Length);

            var bpm = detector.GetBpm();
            if (bpm > 0)
                bpms.Add((int)Math.Round(bpm));
        }

        var clusters = new List<List<int>>();
        foreach (var bpm in bpms.Order())
        {
            if (clusters.Count == 0 || bpm - clusters[^1][^1] > Tolerance)
                clusters.Add([]);

            clusters[^1].Add(bpm);
        }

        var minWindows = Math.Min(MinWindowsPerTrack, windowsCount);
        return clusters.Where(x => x.Count >= minWindows).Select(x => x[x.Count / 2]).ToList();
    }
}
