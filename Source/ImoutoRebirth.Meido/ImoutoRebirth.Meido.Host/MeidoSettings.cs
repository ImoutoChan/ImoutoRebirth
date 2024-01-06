using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Meido.Host;

public class MeidoSettings
{
    public required RabbitSettings RabbitSettings { get; set; }
}
