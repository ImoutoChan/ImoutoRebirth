using ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

namespace ImoutoRebirth.Arachne.Host.Settings
{
    public class ArachneSettings
    {
        public DanbooruSettings DanbooruSettings { get; set; }

        public SankakuSettings SankakuSettings { get; set; }

        public RabbitSettings RabbitSettings { get; set; }
    }
}