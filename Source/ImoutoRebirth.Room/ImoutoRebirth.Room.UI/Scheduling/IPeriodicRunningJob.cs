namespace ImoutoRebirth.Room.UI.Scheduling;

public interface IPeriodicRunningJob
{
    TimeSpan PeriodDelay { get; }

    bool RequestRapidRun { get; }

    Task Run(CancellationToken token);
}
