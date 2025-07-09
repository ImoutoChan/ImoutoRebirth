using ImoutoRebirth.Room.Application.Services;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Room.Infrastructure.Service;

internal class WebhookUploader : IWebhookUploader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookUploader> _logger;

    public WebhookUploader(HttpClient httpClient, ILogger<WebhookUploader> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task UploadFile(string filePath, string uploadUrl)
    {
        _logger.LogInformation("Uploading file {Path} to {UploadUrl}", filePath, uploadUrl);

        try
        {
            var isImage
                = filePath.EndsWith(".jpg")
                  || filePath.EndsWith(".png")
                  || filePath.EndsWith(".jpeg")
                  || filePath.EndsWith(".gif");

            if (!isImage)
                return;

            await using var fileStream = File.OpenRead(filePath);
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "files", Path.GetFileName(filePath));

            using var response = await _httpClient.PostAsync(uploadUrl, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {Path} to {UploadUrl}", filePath, uploadUrl);
        }
    }
}
