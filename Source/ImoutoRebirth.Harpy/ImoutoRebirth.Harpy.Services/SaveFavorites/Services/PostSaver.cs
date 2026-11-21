using System.Web;

namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services;

internal class PostSaver
{
    private readonly HttpClient _httpClient;

    public PostSaver(HttpClient httpClient) => _httpClient = httpClient;

    public async Task SavePost(Post post, string saveDirectoryPath, CancellationToken ct)
    {
        var path = Path.Combine(saveDirectoryPath, HttpUtility.UrlDecode(GetName(post)));
        using var response = await _httpClient.GetAsync(post.FileUrl, ct);
        response.EnsureSuccessStatusCode();

        var file = new FileStream(path, FileMode.CreateNew);
        try
        {
            await using (file)
                await response.Content.CopyToAsync(file, ct);
        }
        catch
        {
            File.Delete(path);
            throw;
        }
    }

    private static string GetName(Post post)
    {
        var extension = post.FileUrl.Split('/').Last().Split('.').Last();
        return $"{post.Md5}{'.'}{extension}";
    }
}
