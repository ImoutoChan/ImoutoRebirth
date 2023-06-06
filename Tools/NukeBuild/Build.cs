using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

public class CustomBuildCmdPathGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomBuildCmdPathGitHubActionsAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images) : base(name, image, images)
    {
    }

    protected override string BuildCmdPath => Build.RootDirectory / "Tools" / "NukeBuild" / "build.cmd";
}

[CustomBuildCmdPathGitHubActions(
    "release",
    GitHubActionsImage.WindowsLatest,
    OnPushTags = new []{ "*"},
    InvokedTargets = new[] { nameof(Pack7ZSfx) },
    AutoGenerate = false)]
class Build : NukeBuild
{
    static readonly IReadOnlyCollection<string> ApplicationProjects = new[]
    {
        "ImoutoRebirth.Arachne.Host",
        "ImoutoRebirth.Harpy.Host",
        "ImoutoRebirth.Kekkai",
        "ImoutoRebirth.Lilin.Host",
        "ImoutoRebirth.Meido.Host",
        "ImoutoRebirth.Navigator",
        "ImoutoRebirth.Room.Webhost",
        "ImoutoRebirth.Tori",
        "ImoutoViewer",
    };

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    //readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    readonly Configuration Configuration = Configuration.Release;

    AbsolutePath SourceDirectory => RootDirectory;
    AbsolutePath OutputDirectory => RootDirectory / "Artifacts";
    
    [Solution] 
    readonly Solution Solution;

    [GitVersion(NoFetch = true)] 
    readonly GitVersion GitVersion;
    
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(x => x.DeleteDirectory());
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testProjects = SourceDirectory.GlobFiles("**/*.Tests.csproj").ToList();

            DotNetTest(t => t
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .CombineWith(testProjects, (_, v) => _.SetProjectFile(v)));
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var output = OutputDirectory / "latest"; 
            output.CreateOrCleanDirectory();

            var apps = Solution.AllProjects.Where(p => ApplicationProjects.Contains(p.Name));

            Parallel.ForEach(apps, project =>
            {
                DotNetPublish(s => s
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetVersion(GitVersion.NuGetVersionV2)
                    .SetOutput(output / project.Directory.Parent!.Name)
                    .SetProject(project));

                if (project.Directory.Parent!.Name == "ImoutoRebirth.Navigator")
                {
                    Directory.Delete(output / project.Directory.Parent!.Name / "de", true);
                    Directory.Delete(output / project.Directory.Parent!.Name / "libvlc" / "win-x86", true);
                }
            });

            CopyFileToOutput("configuration.json");
            CopyFileToOutput("install-update.ps1");
            CopyFileToOutput("install-dependencies.ps1");
            
            return;

            void CopyFileToOutput(string fileName)
            {
                var configFilePath = BuildAssemblyDirectory / fileName;
                var targetConfigFilePath = output / fileName;
                File.Copy(configFilePath, targetConfigFilePath, overwrite: true);
            }
        });
    
    Target Pack7ZSfx => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            var output = OutputDirectory / "latest"; 

            ArchiveAs7Z(output);
            
            return;

            void ArchiveAs7Z(AbsolutePath output)
            {
                var strCmdText =
                    $"""
                    &'C:\Program Files\7-Zip\7z.exe' a -sfx '{output.Parent}\ImoutoRebirth.exe' '{output}\*'
                    """;
                
                Serilog.Log.Warning("CommandToArchive: {Command}", strCmdText);
                
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{strCmdText}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(psi);
                var error = process!.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Serilog.Log.Error(error);
                }
                else
                {
                    Serilog.Log.Information("Archive created");
                }
            }
        });
}
