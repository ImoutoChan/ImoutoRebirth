namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;

internal class BooruConfiguration
{
    public required string Login { get; set; }

    public required string ApiKey { get; set; }

    public required string BotUserAgent { get; set; }
}

internal class YandereBooruConfiguration : BooruConfiguration
{
}

internal class DanbooruBooruConfiguration : BooruConfiguration
{
    public required string UserId { get; set; }

    public string GetUserAgent()
        => int.TryParse(UserId, out var userId) ? $"{BotUserAgent}; user #{userId}" : BotUserAgent;
}

internal class GelbooruBooruConfiguration
{
    public required string ApiKey { get; set; }

    public required string UserId { get; set; }
}
