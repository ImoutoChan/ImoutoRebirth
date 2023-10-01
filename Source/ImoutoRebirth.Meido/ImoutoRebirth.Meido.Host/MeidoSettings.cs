using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Meido.Host;

public class MeidoSettings
{
    public RabbitSettings RabbitSettings { get; set; } = default!;
}
