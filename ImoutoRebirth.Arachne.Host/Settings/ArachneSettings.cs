using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Arachne.Host.Settings;

public class ArachneSettings
{
    public DanbooruSettings DanbooruSettings { get; set; } = default!;

    public SankakuSettings SankakuSettings { get; set; } = default!;

    public RabbitSettings RabbitSettings { get; set; } = default!;
}