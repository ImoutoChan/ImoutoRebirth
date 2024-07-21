using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Arachne.Host.Settings;

public class ArachneSettings
{
    public required DanbooruSettings DanbooruSettings { get; set; }

    public required SankakuSettings SankakuSettings { get; set; }
}
