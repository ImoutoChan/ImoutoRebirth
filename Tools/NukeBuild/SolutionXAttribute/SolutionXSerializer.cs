using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.SolutionPersistence.Serializer;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

// ReSharper disable once CheckNamespace
namespace Nuke.Common.ProjectModel;

internal static class SolutionXSerializer
{
    public static async Task<T> DeserializeFromFile<T>(AbsolutePath solutionFile)
        where T : Solution, new()
    {
        Assert.FileExists(solutionFile);

        await using var file = File.OpenRead(solutionFile);
        var model = await SolutionSerializers.SlnXml.OpenAsync(file, CancellationToken.None);

        await using var oldSolutionFile = new MemoryStream();
        await SolutionSerializers.SlnFileV12.SaveAsync(oldSolutionFile, model, CancellationToken.None);
        oldSolutionFile.Seek(0, SeekOrigin.Begin);
        StreamReader reader = new(oldSolutionFile);
        var oldContent = await reader.ReadToEndAsync();
        var oldContentLined = oldContent.SplitLineBreaks();

        var deserializer = typeof(SolutionSerializer).GetMethod(nameof(SolutionSerializer.DeserializeFromContent)).NotNull()
            .MakeGenericMethod(typeof(T));
        var solution = ((T)deserializer.Invoke(obj: null, new object[] { oldContentLined, solutionFile })).NotNull();

        return solution;
    }
}
