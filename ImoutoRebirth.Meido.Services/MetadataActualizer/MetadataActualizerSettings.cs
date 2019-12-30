using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer
{
    public class MetadataActualizerSettings
    {
        public int RepeatEveryMinutes { get; set; }

        public MetadataSource[] ActiveSources { get; set; }
    }
}