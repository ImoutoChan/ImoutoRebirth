namespace ImoutoRebirth.Meido.Services.FaultTolerance
{
    public class FaultToleranceSettings
    {
        public bool IsEnabled { get; set; } = true;

        public int RepeatEveryMinutes { get; set; } = 60 * 24;
    }
}