using System.Text.RegularExpressions;
using Flurl.Http.Configuration;
using ImoutoRebirth.Arachne.Infrastructure.ExHentai;
using ImoutoRebirth.Arachne.Infrastructure.Schale;
using ImoutoRebirth.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ImoutoRebirth.Hasami;

internal static partial class Scripts
{
    public static async Task SplitBasedOnFoundInExHentai()
    {
        var sourcePath = new DirectoryInfo(@"C:\Playground\DodjiMetaSearching\Processed");
        var targetFoundPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!found"));
        var targetMissPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!miss"));

        var files = sourcePath.GetFiles();

        targetFoundPath.Create();
        targetMissPath.Create();

        int counter = 0;
        foreach (var file in files)
        {
            try
            {
                var isFound = false;
                var found = await FindOnExHentai(file.Name);

                if (found.None())
                {
                    var schaleFound = await FindOnSchale(file.Name);

                    isFound = schaleFound.Any();
                    Console.WriteLine(
                        $"{++counter:000}/{files.Length:000} | {schaleFound.Any().ToString()[0]} ({schaleFound.Count:00}) | {file.Name} | ({schaleFound.FirstOrDefault()?.Title})");
                }
                else
                {
                    isFound = true;
                    Console.WriteLine(
                        $"{++counter:000}/{files.Length:000} | {found.Any().ToString()[0]} ({found.Count:00}) | {file.Name} | ({found.FirstOrDefault()?.Title})");
                }

                var newPath = isFound
                    ? Path.Combine(targetFoundPath.FullName, file.Name)
                    : Path.Combine(targetMissPath.FullName, file.Name);

                file.MoveTo(newPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private static async Task<IReadOnlyCollection<FoundMetadata>> FindOnExHentai(string fileName)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        var provider = new ExHentaiMetadataProvider(
            new FlurlClientCache(),
            new ExHentaiAuthConfig(
                IpbMemberId: configuration.GetValue<string>("ExHentaiSettings:IpbMemberId"),
                IpbPassHash: configuration.GetValue<string>("ExHentaiSettings:IpbPassHash"),
                Igneous: configuration.GetValue<string>("ExHentaiSettings:Igneous"),
                UserAgent: configuration.GetValue<string>("ExHentaiSettings:UserAgent")),
            NullLogger<ExHentaiMetadataProvider>.Instance);

        var name = Path.GetFileNameWithoutExtension(fileName);

        return await provider.DeepSearchMetadataAsync(name);
    }

    private static async Task<IReadOnlyCollection<SchaleMetadata>> FindOnSchale(string fileName)
    {
        var provider = new SchaleMetadataProvider(new FlurlClientCache());

        var name = Path.GetFileNameWithoutExtension(fileName);

        var found = await provider.SearchAsync(name);

        if (found.Any())
            return found;

        name = NameCleanRegex().Replace(name, "");

        return await provider.SearchAsync(name);
    }

    [GeneratedRegex(
        @"(?:(\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})(?=\s*(?:(?:\([^)]*?\)|\[[^]]*?\]|\{[^}]*?\})\s*)*$))",
        RegexOptions.Compiled
        | RegexOptions.IgnoreCase)]
    private static partial Regex NameCleanRegex();
}
