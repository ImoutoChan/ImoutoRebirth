using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Room.Host;

public class RoomSettings
{
    public required RabbitSettings RabbitSettings { get; set; }
}
