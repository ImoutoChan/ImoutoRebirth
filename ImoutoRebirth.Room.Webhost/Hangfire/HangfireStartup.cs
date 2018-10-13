using Hangfire;
using ImoutoRebirth.Room.Core.Services;

namespace ImoutoRebirth.Room.Webhost.Hangfire
{
    internal class HangfireStartup : IHangfireStartup
    {
        public void EnqueueJobs()
        {
            RecurringJob.AddOrUpdate<OverseeService>(service => service.Oversee(), Cron.Minutely);
        }
    }

}
