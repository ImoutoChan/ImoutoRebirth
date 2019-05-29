using System.Threading.Tasks;
using ImoutoRebirth.Common.Quartz;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer
{
    [DisallowConcurrentExecution]
    public class MetaActualizerJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            throw new System.NotImplementedException();
        }

        public class Description : IQuartzJobDescription
        {
            private readonly int _repeatEveryMinutes;

            public Description(IOptions<MetadataActualizerSettings> options)
            {
                _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
            }

            public IJobDetail GetJobDetails()
                => JobBuilder.Create<MetaActualizerJob>()
                             .WithIdentity("MetaActualizer job")
                             .Build();

            public ITrigger GetJobTrigger()
                => TriggerBuilder.Create()
                                 .WithIdentity("Oversee trigger")
                                 .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                                 .Build();
        }
    }
}