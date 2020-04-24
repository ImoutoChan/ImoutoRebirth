using System.Threading.Tasks;
using ImoutoRebirth.Common.Quartz;
using ImoutoRebirth.Meido.Services.FaultTolerance.CqrsCommands;
using ImoutoRebirth.Meido.Services.MetadataActualizer;
using MediatR;
using Microsoft.Extensions.Options;
using Quartz;

namespace ImoutoRebirth.Meido.Services.FaultTolerance
{
    [DisallowConcurrentExecution]
    public class FaultToleranceJob : IJob
    {
        private readonly IMediator _mediator;

        public FaultToleranceJob(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context) 
            => await _mediator.Send(new RequeueFaultsCommand());

        public class Description : IQuartzJobDescription
        {
            private readonly int _repeatEveryMinutes;

            public Description(IOptions<FaultToleranceSettings> options)
            {
                _repeatEveryMinutes = options.Value.RepeatEveryMinutes;
            }

            public IJobDetail GetJobDetails()
                => JobBuilder.Create<FaultToleranceJob>()
                             .WithIdentity("FaultTolerance job")
                             .Build();

            public ITrigger GetJobTrigger()
                => TriggerBuilder.Create()
                                 .WithIdentity("FaultTolerance trigger")
                                 .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever(_repeatEveryMinutes))
                                 .Build();
        }
    }
}