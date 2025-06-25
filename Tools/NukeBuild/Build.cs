using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Z7Tasks;

[CustomBuildCmdPathGitHubActions(
    "release",
    GitHubActionsImage.WindowsLatest,
    OnPushTags = ["*"],
    InvokedTargets = [nameof(Test), nameof(Pack7ZSfx)],
    AutoGenerate = false)]
class Build : NukeBuild
{
    Project[] ProjectsToPublish => new[]
    {
        Solution.ImoutoRebirth_Arachne.ImoutoRebirth_Arachne_Host,
        Solution.ImoutoRebirth_Harpy.ImoutoRebirth_Harpy_Host,
        Solution.ImoutoRebirth_Kekkai.ImoutoRebirth_Kekkai,
        Solution.ImoutoRebirth_Lilin.ImoutoRebirth_Lilin_Host,
        Solution.ImoutoRebirth_Meido.ImoutoRebirth_Meido_Host,
        Solution.ImoutoRebirth_Navigator.ImoutoRebirth_Navigator,
        Solution.ImoutoRebirth_Room.ImoutoRebirth_Room_Host,
        Solution.ImoutoRebirth_Tori.ImoutoRebirth_Tori_UI,
        Solution.Imouto_Viewer.ImoutoViewer
    };

    AbsolutePath[] NukeFilesToPublish => new[]
    {
        BuildAssemblyDirectory / "configuration.json",
        BuildAssemblyDirectory / "install.cmd",
        BuildAssemblyDirectory / "update.cmd"
    };

    Dictionary<string, RelativePath[]> DirectoriesToDeleteForProject => new()
    {
        {
            "ImoutoRebirth.Navigator", new[]
            {
                (RelativePath)"de",
                (RelativePath)"libvlc" / "win-x86"
            }
        }
    };

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Release'")]
    readonly Configuration Configuration = Configuration.Release;

    [Parameter("Inserts version into the artefacts - Default is 'True'")]
    readonly BuildToVersionedFolder VersionedFolder = BuildToVersionedFolder.True;

    AbsolutePath SourceDirectory => RootDirectory;
    
    AbsolutePath OutputDirectory => RootDirectory / "Artifacts";

    AbsolutePath OutputLatestDirectory =>
        OutputDirectory / (VersionedFolder == BuildToVersionedFolder.True ? VersionedName : "latest");
    
    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    [GitVersion(NoFetch = true)]
    readonly GitVersion GitVersion;
    
    string VersionedName => "ImoutoRebirth-" + GitVersion.NuGetVersionV2;
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory
                .GlobDirectories("**/bin", "**/obj")
                .DeleteDirectories();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution)
                .SetVerbosity(DotNetVerbosity.quiet));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetVerbosity(DotNetVerbosity.quiet)
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetVersion(GitVersion.NuGetVersionV2)
                .EnableNoRestore());
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Before(Publish)
        .Executes(() =>
        {
            var testProjects = SourceDirectory.GlobFiles("**/*.Tests.csproj").ToList();

            DotNetTest(t => t
                .SetVerbosity(DotNetVerbosity.quiet)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .CombineWith(testProjects, (_, v) => _.SetProjectFile(v)));
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            OutputLatestDirectory.CreateOrCleanDirectory();

            var publishableProjects =
                from projectToPublish in ProjectsToPublish
                let projectName = projectToPublish.Directory.Parent!.Name
                let projectOutput = OutputLatestDirectory / projectName
                let directoriesToDelete = GetDirectoriesToDelete(projectName, projectOutput).ToList()
                select new
                {
                    Project = projectToPublish,
                    ProjectName = projectName,
                    ProjectOutput = projectOutput,
                    DirectoriesToDelete = directoriesToDelete
                };

            DotNetPublish(s => s
                .SetVerbosity(DotNetVerbosity.quiet)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetVersion(GitVersion.NuGetVersionV2)
                .CombineWith(publishableProjects, (_, v) => _
                    .SetProject(v.Project)
                    .SetOutput(v.ProjectOutput)));

            foreach (var directoryToDelete in publishableProjects.SelectMany(x => x.DirectoriesToDelete))
                directoryToDelete.DeleteDirectory();

            foreach (var nukeFileToPublish in NukeFilesToPublish)
                nukeFileToPublish.CopyToDirectory(OutputLatestDirectory, ExistsPolicy.FileOverwrite);

            return;
            
            AbsolutePath[] GetDirectoriesToDelete(string projectName, AbsolutePath projectOutput)
                => DirectoriesToDeleteForProject
                    .GetValueOrDefault(projectName, [])
                    .Select(x => projectOutput / x)
                    .ToArray();
        });

    Target Pack7ZSfx => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            PackAs7Z(s => s
                .CreateArchive()
                .AsSfx()
                .SetOutputArchiveFile(OutputLatestDirectory.Parent / $"{VersionedName}.exe")
                .SetSourceDirectory(OutputLatestDirectory));
            
            PackAs7Z(s => s
                .CreateArchive()
                .SetOutputArchiveFile(OutputLatestDirectory.Parent / $"{VersionedName}.7z")
                .SetSourceDirectory(OutputLatestDirectory));
        });
    
    Target PrepareChangelog => _ => _
        .Executes(() =>
        {
            var changelog = RootDirectory / "CHANGELOG.md";
            var changelogResult = RootDirectory / "CHANGELOG.RESULT.md";
            var changelogTemplate = RootDirectory / "CHANGELOG.TEMPLATE.md";

            var changelogTemplateContent = File.ReadAllText(changelogTemplate);
            
            var changelogContent = new StringBuilder();
            changelogContent.AppendLine($"## Changes in v{GitVersion.NuGetVersionV2}");
            var empty = true;
            foreach (var changeLogLine in File.ReadLines(changelog))
            {
                if (changeLogLine.StartsWith("# Unreleased"))
                    continue;
                
                if (changeLogLine.StartsWith("# "))
                    break;

                empty = false;
                changelogContent.AppendLine(changeLogLine);
            }
            
            var newChangelogContent = empty
                ? changelogTemplateContent
                : changelogTemplateContent + Environment.NewLine + changelogContent;

            File.WriteAllText(changelogResult, newChangelogContent);
        });
}
