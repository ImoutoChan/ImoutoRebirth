using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Arachne.Host.Settings;

public class ArachneSettings
{
    public required DanbooruSettings DanbooruSettings { get; set; }

    public required GelbooruSettings GelbooruSettings { get; set; }

    public required Rule34Settings Rule34Settings { get; set; }

    public required SankakuSettings SankakuSettings { get; set; }

    public required ExHentaiSettings ExHentaiSettings { get; set; }
}
