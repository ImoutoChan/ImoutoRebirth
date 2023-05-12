using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Lilin.Host.Settings;

public class LilinSettings
{
    public RabbitSettings RabbitSettings { get; set; } = default!;
}