namespace ImoutoRebirth.Common.Jobs;

public interface IRunningJobs<T> where T : Enum
{
    void Add(RunningJob<T> job);

    void Interrupt(T type, Guid id);
}

public record RunningJob<T>(T JobType, Guid Id, CancellationTokenSource Interruptable) where T : Enum;

internal class RunningJobs<TJobType> : IRunningJobs<TJobType>
    where TJobType : Enum
{
    private readonly List<RunningJob<TJobType>> _jobs = new();

    public void Add(RunningJob<TJobType> job)
    {
        lock (_jobs)
            _jobs.Add(job);
    }

    public void Interrupt(TJobType type, Guid id)
    {
        lock (_jobs)
        {
            var job = _jobs.FirstOrDefault(x => x.JobType.Equals(type) && x.Id == id);
            if (job == null)
                return;

            _jobs.Remove(job);
            job.Interruptable.Cancel();
        }
    }
}

