using System.Security.Cryptography;
using ImoutoRebirth.RoomService.WebApi.Client;

var sourcePath = new DirectoryInfo(@"F:\income\NEW\!playground\traps");
var targetFoundPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!found"));
var targetMissPath = new DirectoryInfo(Path.Combine(sourcePath.FullName, "!miss"));

var files = sourcePath.GetFiles();
var hashes = files.Select(x => (File: x, MD5: GetMd5Checksum(x))).ToList();
var hashesOnly = hashes.Select(x => x.MD5).ToList();

// calculate md5

var roomClient = new CollectionFilesClient("http://localhost:11301", new HttpClient());
var found = await roomClient.SearchAsync(new CollectionFilesRequest(null, null, int.MaxValue, hashesOnly, null, 0));

targetFoundPath.Create();
targetMissPath.Create();

foreach (var fileHash in hashes)
{
    var newPath = found.Any(x => x.Md5 == fileHash.MD5)
        ? Path.Combine(targetFoundPath.FullName, fileHash.File.Name)
        : Path.Combine(targetMissPath.FullName, fileHash.File.Name);
    fileHash.File.MoveTo(newPath);
}

string GetMd5Checksum(FileInfo fileInfo)
{
    using var md5 = MD5.Create();
    using var stream = fileInfo.OpenRead();

    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
}
