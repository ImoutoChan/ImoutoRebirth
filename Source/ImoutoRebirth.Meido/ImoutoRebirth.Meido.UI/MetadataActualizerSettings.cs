using ImoutoRebirth.Meido.Domain;

namespace ImoutoRebirth.Meido.UI;

public class MetadataActualizerSettings
{
    public int RepeatEveryMinutes { get; set; }

    public required MetadataSource[] ActiveSources { get; set; }
}
