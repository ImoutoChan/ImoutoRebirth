namespace ImoutoRebirth.Harpy.Services.SaveFavorites.Services.Loaders;

internal class BooruConfiguration
{
    public string Login { get; set; } = default!;

    public string ApiKey { get; set; } = default!;

    public string BotUserAgent { get; set; } = default!;
}

internal class YandereBooruConfiguration : BooruConfiguration
{
}

internal class DanbooruBooruConfiguration : BooruConfiguration
{
}
