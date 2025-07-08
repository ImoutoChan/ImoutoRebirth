namespace ImoutoRebirth.Room.Application.Services;

public interface IWebhookUploader
{
    Task UploadFile(string path, string uploadUrl);
} 