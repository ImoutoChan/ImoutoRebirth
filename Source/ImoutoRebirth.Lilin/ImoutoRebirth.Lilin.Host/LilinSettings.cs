using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Lilin.Host;

public class LilinSettings
{
    public required RabbitSettings RabbitSettings { get; set; }
}
