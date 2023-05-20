using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

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
            OutputDirectory.CreateOrCleanDirectory();

            var apps = Solution.AllProjects.Where(p => ApplicationProjects.Contains(p.Name));

            foreach (var project in apps)
            {
                DotNetPublish(s => s
                    .SetConfiguration(Configuration)
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetVersion(GitVersion.NuGetVersionV2)
                    .SetOutput(OutputDirectory / GitVersion.NuGetVersionV2 / project.Directory.Parent!.Name)
                    .SetProject(project));
            }

            var configFilePath = BuildAssemblyDirectory / "configuration.json";
            var targetConfigFilePath = OutputDirectory / GitVersion.NuGetVersionV2 / "configuration.json";
            File.Copy(configFilePath, targetConfigFilePath, overwrite: true);
        });
}
