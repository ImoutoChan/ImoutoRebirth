using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Arachne.Host.Settings;

public class ArachneSettings
{
    public required DanbooruSettings DanbooruSettings { get; set; }

    public required SankakuSettings SankakuSettings { get; set; }

    public required RabbitSettings RabbitSettings { get; set; }
}