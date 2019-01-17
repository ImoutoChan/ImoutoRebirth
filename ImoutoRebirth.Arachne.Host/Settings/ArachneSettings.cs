using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;
using ImoutoRebirth.Common.MassTransit;

namespace ImoutoRebirth.Arachne.Host.Settings
{
    public class ArachneSettings
    {
        public DanbooruSettings DanbooruSettings { get; set; }

        public SankakuSettings SankakuSettings { get; set; }

        public RabbitSettings RabbitSettings { get; set; }
    }
}