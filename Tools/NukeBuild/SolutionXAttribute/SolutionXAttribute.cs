using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Nuke.Common.IO;
using Nuke.Common.Utilities;
using Serilog;

// ReSharper disable once CheckNamespace
namespace Nuke.Common.ProjectModel;

[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Assign)]
public class SolutionXAttribute : ParameterAttribute
{
    private readonly string _relativePath;

    public SolutionXAttribute()
        : this(relativePath: null)
    {
    }

    public SolutionXAttribute(string relativePath)
        : base("Path to a solution file that is automatically loaded."
               + (relativePath != null ? $" Default is {relativePath}." : string.Empty))
    {
        _relativePath = relativePath;
    }

    public override bool List { get; set; }

    public bool GenerateProjects { get; set; }

    public bool SuppressBuildProjectCheck { get; set; }

    public override object GetValue(MemberInfo member, object instance)
    {
        var solutionFile = TryGetSolutionFileFromNukeFile() ??
                           GetSolutionFileFromParametersFile(member);
        var deserializer = typeof(SolutionXSerializer).GetMethod(nameof(SolutionXSerializer.DeserializeFromFile)).NotNull()
            .MakeGenericMethod(member.GetMemberType());
        var solutionTask = ((Task<SolutionX>)deserializer.Invoke(obj: null, new object[] { solutionFile })).NotNull();
        var solution = (Solution)solutionTask.GetAwaiter().GetResult();
        // var solution = SolutionXSerializer.DeserializeFromFile<Solution>(solutionFile)

        if (!SuppressBuildProjectCheck)
        {
            var buildProject = solution.AllProjects.SingleOrDefault(x => x.Directory.Equals(Build.BuildProjectDirectory));
            var buildProjectConfigurations = buildProject?.Configurations.Where(x => x.Key.Contains("Build")).ToList();

            if (buildProject != null && buildProjectConfigurations.Any())
            {
                Log.Warning(
                    "Solution {Solution} has active build configurations for the build project.\n" +
                    $"Either enable {nameof(SuppressBuildProjectCheck)} on {{Member}} or remove the following entries from the solution file:\n" +
                    "{Entries}",
                    solution,
                    member.GetDisplayName(),
                    buildProjectConfigurations.Select(x => $"  - {buildProject.ProjectId.ToString("B").ToUpper()}.{x.Key} = {x.Value}").JoinNewLine());
            }
        }

        return solution;
    }

    private AbsolutePath GetSolutionFileFromParametersFile(MemberInfo member)
    {
        return _relativePath != null
            ? Build.RootDirectory / _relativePath
            : ParameterService.GetParameter<AbsolutePath>(member).NotNull($"No solution file defined for '{member.Name}'.");
    }

    private AbsolutePath TryGetSolutionFileFromNukeFile()
    {
        var nukeFile = Build.RootDirectory / Constants.NukeFileName;
        if (!nukeFile.Exists())
            return null;

        var solutionFileRelative = nukeFile.ReadAllLines().ElementAtOrDefault(0);
        Assert.True(solutionFileRelative != null && !solutionFileRelative.Contains(value: '\\'),
            $"First line of {Constants.NukeFileName} must provide solution path using UNIX separators");

        var solutionFile = Build.RootDirectory / solutionFileRelative;
        Assert.FileExists(solutionFile, $"Solution file '{solutionFile}' provided via {Constants.NukeFileName} does not exist");

        return solutionFile;
    }
}
