namespace ImoutoRebirth.Meido.UI;

public class FaultToleranceSettings
{
    public bool IsEnabled { get; set; } = false;

    public int RepeatEveryMinutes { get; set; } = 60 * 24;
}
