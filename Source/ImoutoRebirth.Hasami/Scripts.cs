using System.Security.Cryptography;
using Imouto.BooruParser;
using Imouto.BooruParser.Implementations.Danbooru;
using Imouto.BooruParser.Implementations.Sankaku;
using ImoutoRebirth.RoomService.WebApi.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Hasami;

internal static class Scripts
{
    public static async Task SplitBasedOnSaved()
    {
        var sourcePath = new DirectoryInfo(@"F:\income\NEW\!playground\traps");
        var targetFoundPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!found"));
        var targetMissPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!miss"));

        var files = sourcePath.GetFiles();
        var hashes = files.Select(x => (File: x, MD5: GetMd5Checksum(x))).ToList();
        var hashesOnly = hashes.Select(x => x.MD5).ToList();

        var roomClient = new CollectionFilesClient("http://localhost:11301", new HttpClient());
        var found = await roomClient.SearchCollectionFilesAsync(new CollectionFilesQuery(null, null, int.MaxValue, hashesOnly, null, 0));

        targetFoundPath.Create();
        targetMissPath.Create();

        foreach (var fileHash in hashes)
        {
            var newPath = found.Any(x => x.Md5 == fileHash.MD5)
                ? Path.Combine(targetFoundPath.FullName, fileHash.File.Name)
                : Path.Combine(targetMissPath.FullName, fileHash.File.Name);
            fileHash.File.MoveTo(newPath);
        }
    }

    public static async Task SplitBasedOnFoundInBooru()
    {
        
        var sourcePath = new DirectoryInfo(@"C:\Users\Oniii-chan\Downloads\pepper0\!new\!pngs");
        var targetFoundPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!found"));
        var targetMissPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!miss"));

        var files = sourcePath.GetFiles();
        var hashes = files.Select(x => (File: x, MD5: GetMd5Checksum(x))).ToList();
        var hashesOnly = hashes.Select(x => x.MD5).ToList();
        
        targetFoundPath.Create();
        targetMissPath.Create();

        int counter = 0;
        foreach (var fileHash in hashes)
        {
            try
            {
                var found = await IsFoundInAnyBooru(fileHash.MD5);
            
                var newPath = found
                    ? Path.Combine(targetFoundPath.FullName, fileHash.File.Name)
                    : Path.Combine(targetMissPath.FullName, fileHash.File.Name);
                
                Console.WriteLine($"{fileHash.File.Name} - {found} {++counter}/{hashes.Count}");
                
                fileHash.File.MoveTo(newPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private static async Task<bool> IsFoundInAnyBooru(string md5)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();
        
        var services = new ServiceCollection();
        services.AddBooruParsers();
        services.AddMemoryCache();
        services.Configure<DanbooruSettings>(x =>
        {
            x.Login = configuration.GetValue<string>("DanbooruSettings:Login");
            x.ApiKey = configuration.GetValue<string>("DanbooruSettings:ApiKey");
            x.PauseBetweenRequestsInMs = 0;
            x.BotUserAgent = configuration.GetValue<string>("DanbooruSettings:UserAgent");
        });
        services.Configure<SankakuSettings>(x =>
        {
            x.Login = configuration.GetValue<string>("SankakuSettings:Login");
            x.Password = configuration.GetValue<string>("SankakuSettings:Password");
            x.PauseBetweenRequestsInMs = 5000;
        });
        var provider = services.BuildServiceProvider();

        var booruLoaders = provider.GetRequiredService<IEnumerable<IBooruApiLoader>>();

        var tasks = booruLoaders.Select(x => x.SearchAsync($"md5:{md5}"));
        
        var results = await Task.WhenAll(tasks);

        return results.Any(x => x.Results.Any());
    }

    private static string GetMd5Checksum(FileInfo fileInfo)
    {
        using var md5 = MD5.Create();
        using var stream = fileInfo.OpenRead();

        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
    }
}
