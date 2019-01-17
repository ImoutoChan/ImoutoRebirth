using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace ImoutoRebirth.Common.Quartz
{
    internal class QuartzStartup
    {
        private const int WaitBeforeForcedShutdown = 30000;

        private readonly IScheduler _scheduler;
        private readonly IReadOnlyCollection<IQuartzJobDescription> _jobDescriptions;

        public QuartzStartup(IScheduler scheduler, IEnumerable<IQuartzJobDescription> jobDescriptions)
        {
            _scheduler = scheduler;
            _jobDescriptions = jobDescriptions.ToArray();
        }

        public void Start()
        {
            StartAsync().Wait();
        }

        public async Task StartAsync()
        {
            await _scheduler.Start();

            foreach (var jobDescription in _jobDescriptions)
                await _scheduler.ScheduleJob(jobDescription.GetJobDetails(), jobDescription.GetJobTrigger());
        }

        public void Stop()
        {
            _scheduler?.Shutdown(waitForJobsToComplete: true)
                       .Wait(WaitBeforeForcedShutdown);
        }
    }
}