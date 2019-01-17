using Quartz;

namespace ImoutoRebirth.Common.Quartz.Extensions
{
    public static class SchedulerFactoryExtensions
    {
        public static IScheduler GetSchedulerSynchronously(this ISchedulerFactory factory)
            => factory.GetScheduler().Result;
    }
}