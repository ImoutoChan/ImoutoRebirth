namespace ImoutoRebirth.Room.UI.Scheduling;

public interface IPeriodicRunningJob
{
    TimeSpan PeriodDelay { get; }
    
    Task Run(CancellationToken token);
}
