using ImoutoRebirth.Common;
using Semver;

namespace ImoutoRebirth.Navigator.Slices.Updates.Models;

public class ReleaseInfo
{
    public required string TagName { get; init; }

    public required SemVersion Version { get; init; }

    public required DateTimeOffset PublishedAt { get; init; }

    public required string InstallerUrl { get; init; }

    public required bool IsPrerelease { get; init; }

    public string VersionDisplay => Version.ToString();

    public static ReleaseInfo? FromGitHubRelease(GitHubRelease release)
    {
        var installerAsset = release.Assets
            .FirstOrDefault(a => a.Name.StartsWithIgnoreCase("ImoutoRebirth") && a.Name.EndsWithIgnoreCase(".exe"));

        if (installerAsset is null)
            return null;

        var version = release.TagName.EndsWithIgnoreCase("v") ? release.TagName[1..] : release.TagName;

        if (!SemVersion.TryParse(version, SemVersionStyles.Any, out var semver))
            return null;

        return new ReleaseInfo
        {
            TagName = release.TagName,
            Version = semver,
            PublishedAt = release.PublishedAt,
            InstallerUrl = installerAsset.BrowserDownloadUrl,
            IsPrerelease = release.Prerelease
        };
    }

    public bool IsNewerThan(SemVersion currentVersion) => Version > currentVersion;
}
