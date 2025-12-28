using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using ImoutoRebirth.Navigator.Slices.Updates.Models;
using Semver;

namespace ImoutoRebirth.Navigator.Slices.Updates.Services;

internal sealed class UpdateService : IUpdateService
{
    private const string GitHubApiBaseUrl = "https://api.github.com/repos/ImoutoChan/ImoutoRebirth/releases";

    private readonly HttpClient _httpClient;

    public UpdateService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<ReleaseInfo?> GetLatestStableAsync(CancellationToken ct = default)
    {
        var release = await _httpClient.GetFromJsonAsync<GitHubRelease>(
            $"{GitHubApiBaseUrl}/latest",
            ct);

        return release is null ? null : ReleaseInfo.FromGitHubRelease(release);
    }

    public async Task<ReleaseInfo?> GetLatestAlphaAsync(CancellationToken ct = default)
    {
        var releases = await _httpClient.GetFromJsonAsync<GitHubRelease[]>(
            GitHubApiBaseUrl,
            ct);

        if (releases is null)
            return null;

        var latestPrerelease = releases
            .OrderByDescending(r => r.PublishedAt)
            .FirstOrDefault();

        return latestPrerelease is null ? null : ReleaseInfo.FromGitHubRelease(latestPrerelease);
    }

    public async Task DownloadInstallerAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken ct = default)
    {
        var file = new System.IO.FileInfo(destinationPath);
        file.Directory?.Delete(true);
        file.Directory?.Create();

        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            ct);

        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var downloadedBytes = 0L;

        await using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        await using var fileStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        var buffer = new byte[81920];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, ct)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
            downloadedBytes += bytesRead;

            if (totalBytes > 0 && progress is not null)
            {
                var percentage = (double)downloadedBytes / totalBytes * 100;
                progress.Report(percentage);
            }
        }
    }

    public SemVersion GetCurrentVersion()
    {
        var semVer = Assembly
            .GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (semVer is not null && SemVersion.TryParse(semVer, SemVersionStyles.Any, out var version))
            return version;

        var assemblyVersion = Assembly
            .GetEntryAssembly()
            ?.GetName().Version;

        if (assemblyVersion is not null)
        {
            return new SemVersion(
                assemblyVersion.Major,
                assemblyVersion.Minor,
                assemblyVersion.Build);
        }

        return new SemVersion(0, 0, 0);
    }
}
