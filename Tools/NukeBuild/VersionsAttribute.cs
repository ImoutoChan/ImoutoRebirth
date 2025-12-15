using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Nuke.Common.Tools.Git;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Nuke.Common.ValueInjection;

public partial class VersionsAttribute : ValueInjectionAttributeBase
{
    public override object GetValue(MemberInfo member, object instance)
    {
        try
        {
            var tag = GitTasks.Git("describe --tags --exact-match")
                .Select(x => x.Text.Trim())
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var semVersion = tag.StartsWithOrdinalIgnoreCase("v") ? tag[1..] : tag;
                var parts = SemVersionRegex().Match(semVersion);

                return new VersionsInfo(semVersion,
                    parts.Groups["major"].Value
                    + "."
                    + parts.Groups["minor"].Value
                    + "."
                    + parts.Groups["patch"].Value);
            }
        }
        catch
        {
            // ignored
        }

        var gitVersionGetter = new GitVersionAttribute { NoFetch = true };
        var gitVersion = (GitVersion)gitVersionGetter.GetValue(null, null)!;
        return new VersionsInfo(gitVersion.FullSemVer, gitVersion.AssemblySemVer);
    }

    [GeneratedRegex(
        """
        ^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$
        """, RegexOptions.Compiled)]
    private static partial Regex SemVersionRegex();
}

internal record VersionsInfo(string FullSemVersion, string AssemblyVersion);
