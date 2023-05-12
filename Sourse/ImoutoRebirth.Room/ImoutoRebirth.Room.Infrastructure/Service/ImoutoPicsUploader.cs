using ImoutoRebirth.Room.Core.Services.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Infrastructure.Service;

public class ImoutoPicsUploader : IImoutoPicsUploader
{
    private readonly HttpClient _httpClient;
    private readonly string _imoutoPicsUploadUrl;
    private readonly ILogger<ImoutoPicsUploader> _logger;

    public ImoutoPicsUploader(HttpClient httpClient, ILogger<ImoutoPicsUploader> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;

        _imoutoPicsUploadUrl
            = configuration.GetValue<string>("ImoutoPicsUploadUrl") ??
              throw new Exception("Missing ImoutoPicsUploadUrl");
    }

    public async Task UploadFile(string filePath)
    {
        var isImage = filePath.EndsWith(".jpg")
                      || filePath.EndsWith(".png")
                      || filePath.EndsWith(".jpeg")
                      || filePath.EndsWith(".gif");
        
        if (!isImage)
            return;

        try
        {
            await using var fileStream = File.OpenRead(filePath);
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "files", Path.GetFileName(filePath));

            using var response = await _httpClient.PostAsync(_imoutoPicsUploadUrl, content);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error uploading file");
        }
    }
}
