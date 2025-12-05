namespace ImoutoRebirth.Common.Jobs;

public interface IRunningJobs<T> where T : Enum
{
    void Add(RunningJob<T> job);

    void Interrupt(T type);
}

public record RunningJob<T>(T JobType, CancellationTokenSource Interruptable) where T : Enum;

internal class RunningJobs<TJobType> : IRunningJobs<TJobType>
    where TJobType : Enum
{
    private readonly List<RunningJob<TJobType>> _jobs = new();

    public void Add(RunningJob<TJobType> job) => _jobs.Add(job);

    public void Interrupt(TJobType type)
    {
        var job = _jobs.FirstOrDefault(x => x.JobType.Equals(type));

        if (job == null)
            return;

        if (!job.Interruptable.IsCancellationRequested)
            job.Interruptable.Cancel();

        _jobs.Remove(job);
    }
}

