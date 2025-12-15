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
    Project[] ProjectsToPublish =>
    [
        SolutionX.ImoutoRebirth_Arachne.ImoutoRebirth_Arachne_Host,
        SolutionX.ImoutoRebirth_Harpy.ImoutoRebirth_Harpy_Host,
        SolutionX.ImoutoRebirth_Kekkai.ImoutoRebirth_Kekkai,
        SolutionX.ImoutoRebirth_Lilin.ImoutoRebirth_Lilin_Host,
        SolutionX.ImoutoRebirth_Meido.ImoutoRebirth_Meido_Host,
        SolutionX.ImoutoRebirth_Navigator.ImoutoRebirth_Navigator,
        SolutionX.ImoutoRebirth_Room.ImoutoRebirth_Room_Host,
        SolutionX.ImoutoRebirth_Tori.ImoutoRebirth_Tori_UI,
        SolutionX.Imouto_Viewer.ImoutoViewer,
        SolutionX.ImoutoRebirth_Lamia.ImoutoRebirth_Lamia_Host
    ];

    AbsolutePath[] NukeFilesToPublish =>
    [
        BuildAssemblyDirectory / "configuration.json",
        BuildAssemblyDirectory / "install.cmd"
    ];

    Dictionary<string, RelativePath[]> DirectoriesToDeleteForProject => new()
    {
        {
            "ImoutoRebirth.Navigator", [
                (RelativePath)"de",
                (RelativePath)"libvlc" / "win-x86"
            ]
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
    readonly SolutionX SolutionX;

    [GitVersion(NoFetch = true)]
    readonly GitVersion GitVersion;
    
    string VersionedName => "ImoutoRebirth-" + GitVersion.FullSemVer;
    
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
            DotNetRestore(s => s.SetProjectFile(SolutionX)
                .SetVerbosity(DotNetVerbosity.quiet));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetVerbosity(DotNetVerbosity.quiet)
                .SetProjectFile(SolutionX)
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
            var testProjects = SourceDirectory.GlobFiles("**/*.Tests.csproj")
                .Where(x => x.Name != "ImoutoRebirth.Common.Tests.csproj")
                .ToList();

            DotNetTest(t => t
                .SetVerbosity(DotNetVerbosity.quiet)
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .SetFilter("ExternalResourceRequired!=True")
                .CombineWith(testProjects, (_, v) => _.SetProjectFile(v)));
        });

    Target Publish => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            OutputLatestDirectory.CreateOrCleanDirectory();

            var publishableProjects =
            (
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
                }
            ).ToList();

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

            foreach (var appSettingsFile in publishableProjects
                         .SelectMany(x => x.ProjectOutput.GetFiles("appsettings*")))
            {
                appSettingsFile.DeleteFile();
            }

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
