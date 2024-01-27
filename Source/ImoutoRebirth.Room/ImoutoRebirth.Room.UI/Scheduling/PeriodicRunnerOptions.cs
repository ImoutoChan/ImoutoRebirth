namespace ImoutoRebirth.Room.UI.Scheduling;

internal class PeriodicRunnerOptions
{
    public IReadOnlyCollection<Type> JobTypes { get; set; } = new List<Type>();
    
    public void AddJob<TJob>()
        where TJob : IPeriodicRunningJob
    {
        ((List<Type>)JobTypes).Add(typeof(TJob));
    }
}
