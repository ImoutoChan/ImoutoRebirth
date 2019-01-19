using System.Threading.Tasks;
using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Room.Core.Services.Abstract;
using Quartz;

namespace ImoutoRebirth.Room.Webhost.Quartz
{
    public class OverseeJob : IJob
    {
        private readonly IOverseeService _overseeService;

        public OverseeJob(IOverseeService overseeService)
        {
            _overseeService = overseeService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _overseeService.Oversee();
        }

        public class Description : IQuartzJobDescription
        {
            public IJobDetail GetJobDetails()
                => JobBuilder.Create<OverseeJob>()
                             .WithIdentity("Oversee job")
                             .Build();

            public ITrigger GetJobTrigger()
                => TriggerBuilder.Create()
                                 .WithIdentity("Oversee trigger")
                                 .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(15))
                                 .Build();
        }
    }
}
