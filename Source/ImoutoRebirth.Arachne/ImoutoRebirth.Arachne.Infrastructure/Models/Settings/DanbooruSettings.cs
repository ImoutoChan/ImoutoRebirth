namespace ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

public class DanbooruSettings
{
    public required string Login { get; set; }

    public required string ApiKey { get; set; }

    public int Delay { get; set; }

    public required string BotUserAgent { get; set; }
}
