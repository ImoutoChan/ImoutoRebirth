using ImoutoRebirth.Meido.Domain;

namespace ImoutoRebirth.Meido.UI;

public class MetadataActualizerSettings
{
    public int RepeatEveryMinutes { get; set; }

    public MetadataSource[]? ActiveSources { get; set; }
}
