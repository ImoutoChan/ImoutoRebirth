using ImoutoRebirth.Navigator.Slices.Updates.Models;
using Semver;

namespace ImoutoRebirth.Navigator.Slices.Updates.Services;

internal interface IUpdateService
{
    Task<ReleaseInfo?> GetLatestStableAsync(CancellationToken ct = default);

    Task<ReleaseInfo?> GetLatestAlphaAsync(CancellationToken ct = default);

    Task DownloadInstallerAsync(
        string url,
        string destinationPath,
        IProgress<double>? progress = null,
        CancellationToken ct = default);

    SemVersion GetCurrentVersion();
}
