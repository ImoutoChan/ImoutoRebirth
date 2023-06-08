using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Lilin.Host.Settings;

public class LilinSettings
{
    public required RabbitSettings RabbitSettings { get; set; }
}
