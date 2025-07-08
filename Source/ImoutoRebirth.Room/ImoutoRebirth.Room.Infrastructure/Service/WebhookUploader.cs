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

    public async Task UploadFile(string path, string uploadUrl)
    {
        _logger.LogInformation("Uploading file {Path} to {UploadUrl}", path, uploadUrl);

        try
        {
            using var fileContent = new MultipartFormDataContent();
            await using var fileStream = File.OpenRead(path);
            var streamContent = new StreamContent(fileStream);
            fileContent.Add(streamContent, "file", Path.GetFileName(path));

            var response = await _httpClient.PostAsync(uploadUrl, fileContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("File {Path} uploaded successfully", path);
            }
            else
            {
                _logger.LogWarning("Failed to upload file {Path}. Status: {StatusCode}", path, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {Path} to {UploadUrl}", path, uploadUrl);
        }
    }
} 