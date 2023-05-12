using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Meido.Host.Settings;

public class MeidoSettings
{
    public RabbitSettings RabbitSettings { get; set; } = default!;
}